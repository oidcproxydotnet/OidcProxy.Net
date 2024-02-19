using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server;
using Xunit;

namespace Host.TestApps.OpenIdDict.IntegrationTests.Fixtures.OpenIddict;

public class OpenIddictFixture : IAsyncLifetime, IDisposable
{
    private WebApplication app;
    
    public async Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder();

        builder.Services.AddCors();
        builder.Services.AddControllersWithViews();

        builder.Services.AddDbContext<DbContext>(options =>
        {
            options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-oidcproxy-test-server.sqlite3")}");
            options.UseOpenIddict();
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
            .AddEntityFrameworkStores<DbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddOpenIddict()
            .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<DbContext>())
            .AddServer(options =>
            {
                options.SetTokenEndpointUris("connect/token");

                options.SetAuthorizationEndpointUris("connect/authorize");

                options
                    .AllowAuthorizationCodeFlow()
                    .AllowRefreshTokenFlow();

                AddEncryptionMethod(options);
        
                options
                    .AddDevelopmentSigningCertificate();

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
        
        app = builder.Build();
        
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

        app.Urls.Add("https://localhost:8765/");
        
        await app.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await app.StopAsync();
    }

    public void Dispose()
    {
        // I.l.e.
    }
    
    protected virtual void AddEncryptionMethod(OpenIddictServerBuilder options)
    {
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));
    }
}