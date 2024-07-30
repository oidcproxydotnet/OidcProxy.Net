using System.Security.Cryptography.X509Certificates;
using Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet.OpenIdConnectImplementations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

public class OidcProxyBuilder
{
    public string Url { get; private set; } = "https://localhost:8444";
    private bool _allowAnonymousAccess = true;
    private bool _addClaimsTransformation;
    private bool _addSigningKey;
    private bool _authenticateOnlyMode;
    private List<string> _whitelist = new();
    private IEncryptionKey? _encryptionKey = null;
    private Action<WebApplicationBuilder> _configurePolicyOnWebAppBuilder = _ => { };
    private Action<WebApplication> _configurePolicyOnWebApplication = _ => { };
    private MockedOpenIdConnectIdentityProviderSettings _settings = new();

    public OidcProxyBuilder WithUrl(string url)
    {
        Url = url;
        return this;
    }

    public OidcProxyBuilder AllowAnonymousAccess(bool allowAnonymousAccess)
    {
        _allowAnonymousAccess = allowAnonymousAccess;
        return this;
    }
    
    public OidcProxyBuilder WhitelistRedirectUrl(string url)
    {
        _whitelist.Add(url);
        return this;
    }
    
    public OidcProxyBuilder WithClaimsTransformation()
    {
        _addClaimsTransformation = true;
        return this;
    }

    public OidcProxyBuilder WithSigningKey()
    {
        _addSigningKey = true;
        return this;
    }

    public OidcProxyBuilder WithMitm(AbuseCase abuseCase)
    {
        if ((abuseCase & AbuseCase.TamperedPayload) != 0)
        {
            _settings.TamperedPayload = true;
        }
        
        if ((abuseCase & AbuseCase.ChangedAlgorithm) != 0)
        {
            _settings.AlgorithmChanged = true;
        }
        
        if ((abuseCase & AbuseCase.RemovedHeader) != 0)
        {
            _settings.WithNoHeader = true;
        }
        
        if ((abuseCase & AbuseCase.TrailingDots) != 0)
        {
            _settings.WithTrailingDots = true;
        }

        return this;
    }

    public OidcProxyBuilder WithExpiredAccessToken()
    {
        _settings.WithExpiredToken = true;
        return this;
    }
    
    public OidcProxyBuilder WithEncryptionKey(IEncryptionKey key)
    {
        _encryptionKey = key;
        return this;
    }
    
    public OidcProxyBuilder WithJweCert(X509Certificate2 cert)
    {
        _encryptionKey = new SslCertificate(cert);
        return this;
    }

    public OidcProxyBuilder WithPolicy()
    {
        const string policyName = "foo";
        
        _configurePolicyOnWebAppBuilder = builder =>
        {
            builder.Services.AddAuthorization(o =>
            {
                o.AddPolicy(policyName, policy => { policy.Requirements.Add(new DummyClaimRequirement()); });
            });
            
            builder.Services.AddSingleton<IAuthorizationHandler, DummyClaimHandler>();
        };

        _configurePolicyOnWebApplication = app =>
        {
            app.Use(async (context, next) =>
            {
                var authService = context.RequestServices.GetRequiredService<IAuthorizationService>();

                var result = await authService.AuthorizeAsync(context.User, null, policyName);

                if (!result.Succeeded)
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    await context.Response.WriteAsync("Forbidden");
                    return;
                }

                await next();
            });
        };

        return this;
    }
    
    public void WithAuthenticateOnlyMode()
    {
        _authenticateOnlyMode = true;
    }

    public WebApplication Build()
    {
        // Reset
        MockedOpenIdConnectIdentityProvider.HasRefreshedToken = false;
        OidcProxy.Net.ModuleInitializers.ModuleInitializer.Reset();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        
        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();

        config!.AllowAnonymousAccess = _allowAnonymousAccess;

        if (_authenticateOnlyMode)
        {
            config.ReverseProxy = null;
            config.Mode = Mode.AuthenticateOnly;
        }

        if (_whitelist.Any())
        {
            config.AllowedLandingPages = _whitelist;
        }

        builder.Services.AddOidcProxy(config, x =>
        {
            if (_addClaimsTransformation)
            {
                x.AddClaimsTransformation<ClaimsTransformation>();
            }

            if (_encryptionKey != null)
            {
                x.UseEncryptionKey(_encryptionKey);
            }

            if (_addSigningKey)
            {
                var bytes = "fImIhRwPzldBm0w4rNQGv0FQ5O1ArMgH+6zT4AlSbgE=\n"u8.ToArray();
                x.UseSigningKey(new SymmetricKey(new SymmetricSecurityKey(bytes)));
            }
        });
        
        _configurePolicyOnWebAppBuilder.Invoke(builder);
        
        // Mock the default identity provider with a mock..
        builder.Services
            .AddTransient<IIdentityProvider, MockedOpenIdConnectIdentityProvider>()
            .AddTransient(_ => _settings)
            .AddHttpClient<MockedOpenIdConnectIdentityProvider>();;
        
        var app = builder.Build();
        
        app
            .MapGet("/custom/me", async context => await context.Response.WriteAsJsonAsync(context.User.Identity?.Name))
            .RequireAuthorization();

        app
            .MapGet("/", () => "{}");
        
        app.UseOidcProxy();
        
        _configurePolicyOnWebApplication.Invoke(app);

        app.Urls.Add(Url);

        return app;
    }
}