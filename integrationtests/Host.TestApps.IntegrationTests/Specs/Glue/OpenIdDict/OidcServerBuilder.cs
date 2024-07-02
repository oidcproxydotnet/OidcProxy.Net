using System.Security.Cryptography.X509Certificates;
using System.Text;
using Host.TestApps.IntegrationTests.Fixtures.OpenIddict;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OpenIdDict;

public class OidcServerBuilder
{
    private string _url = "https://localhost:8765/";

    private Action<OpenIddictServerBuilder> _configureEncryptionMethod = options =>
    {
        options.DisableAccessTokenEncryption();

        options.AddDevelopmentEncryptionCertificate();
    };

    private Action<OpenIddictServerBuilder> _configureSigningAlgorithm = options => options.AddDevelopmentSigningCertificate();

    public OidcServerBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }
    
    public void WithJweKey(SymmetricSecurityKey encryptionKey)
    {
        _configureEncryptionMethod = options =>
        {
            options.AddEncryptionKey(encryptionKey);
        };
    }

    public void WithJweCert(X509Certificate2 x509Certificate2)
    {
        _configureEncryptionMethod = options =>
        {
            options.AddEncryptionCertificate(x509Certificate2);
        };
    }

    public void UseRS256Algorithm()
    {
        // do nothing.. RS256 is used by default..
    }
    
    public void UseHS256Algorithm()
    {
        _configureSigningAlgorithm = options =>
        {
            var keyBytes = "fImIhRwPzldBm0w4rNQGv0FQ5O1ArMgH+6zT4AlSbgE=\n"u8.ToArray();
            options
                .AddEphemeralSigningKey()
                .AddSigningKey(new SymmetricSecurityKey(keyBytes));
        };
    }

    public WebApplication Build()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddCors();
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<TestDbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-oidcproxy-test-server.sqlite3")}");
            options.UseOpenIddict();
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<TestDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddOpenIddict()
            .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<TestDbContext>())
            .AddServer(options =>
            {   
                options.SetTokenEndpointUris("connect/token");

                options.SetAuthorizationEndpointUris("connect/authorize");

                options
                    .AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                _configureEncryptionMethod(options);

                _configureSigningAlgorithm.Invoke(options);

                options
                    .UseAspNetCore()
                    .EnableTokenEndpointPassthrough()
                    .EnableAuthorizationEndpointPassthrough();
                
                options
                    .RemoveEventHandler(OpenIddictServerHandlers.Exchange.ValidateScopeParameter.Descriptor);
            })

            .AddValidation(options =>
            {
                options.UseLocalServer();
                options.UseAspNetCore();
            });

        builder.Services.AddHostedService<Worker>();

        builder.ConfigureDotnetDevCertExplicitlyIfItExists(_url);
        
        var app = builder.Build();
        
        app.UseDeveloperExceptionPage();

        app.UseRouting();
        app.UseCors();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseEndpoints(options =>
        {
            options.MapControllers();
            options.MapDefaultControllerRoute();
        });

        app
            .MapAuthorizeEndpoint()
            .MapTokenEndpoint()
            .MapLogoutEndpoint();

        app.Urls.Add(_url);

        return app;
    }

}