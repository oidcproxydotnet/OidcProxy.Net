using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.OpenIdConnect.Jwe;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

public class OidcProxyBuilder
{
    public string Url { get; private set; } = "https://localhost:8444";
    private bool _allowAnonymousAccess = true;
    private bool _addClaimsTransformation = false;
    private List<string> _whitelist = new();
    private IJweEncryptionKey? _encryptionKey = null;
    private Action<WebApplicationBuilder> _configurePolicyOnWebAppBuilder = _ => { };
    private Action<WebApplication> _configurePolicyOnWebApplication = _ => { };

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

    public OidcProxyBuilder WithJweKey(IJweEncryptionKey key)
    {
        _encryptionKey = key;
        return this;
    }
    
    public OidcProxyBuilder WithJweCert(X509Certificate2 cert)
    {
        _encryptionKey = new EncryptionCertificate(cert);
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

    public WebApplication Build()
    {
        OidcProxy.Net.ModuleInitializers.ModuleInitializer.Reset();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        
        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();

        config.AllowAnonymousAccess = _allowAnonymousAccess;

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
                x.UseJweKey(_encryptionKey);
            }
        });
        
        _configurePolicyOnWebAppBuilder.Invoke(builder);

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