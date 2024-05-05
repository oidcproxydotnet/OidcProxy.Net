using System.Net;
using FluentAssertions;
using Host.TestApps.OpenIdDict.IntegrationTests.Fixtures;
using Host.TestApps.OpenIdDict.IntegrationTests.Fixtures.OpenIddict;
using Host.TestApps.OpenIdDict.IntegrationTests.Pom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using Xunit;

namespace Host.TestApps.OpenIdDict.IntegrationTests;

public class BlockAnonymousAccessTest : IAsyncLifetime, 
    IClassFixture<EchoFixture>, // Run an echo-api
    IClassFixture<IdentityServerMimicFixture> // Run an oidc-server
{
    private WebApplication? _testApi = null;
    
    public async Task InitializeAsync()
    {
        OidcProxy.Net.ModuleInitializers.ModuleInitializer.Reset();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        
        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();

        // Deny anonymous access 
        config.AllowAnonymousAccess = false;
        
        builder.Services.AddOidcProxy(config);

        _testApi = builder.Build();
        
        _testApi
            .MapGet("/custom/me", async context => await context.Response.WriteAsJsonAsync(context.User.Identity?.Name))
            .RequireAuthorization();

        _testApi
            .MapGet("/", () => "{}");
        
        _testApi.UseOidcProxy();

        _testApi.Urls.Add("https://localhost:8444");

        await _testApi.StartAsync();
    }

    [Fact]
    public async Task ItShouldAuthenticateFirst()
    {
        const string subJohnDoe = "johndoe";

        var app = new App();

        try
        {
            const string path = "/api/echo/?foo=bar#test=123";
            
            // Navigate to the cho endpoint
            await app.NavigateToProxy(path);
            await Task.Delay(2500);
            
            // Because the Proxy is configured to disallow anonymous access, it should kick the user to the IDP.
            // The test-idp has been configured to authenticate users automatically. Consequently, there are various
            // redirects involved. Anyhow, the user should be redirected back to the URL they initially requested.
            app.CurrentUrl.Should().EndWith(path);
            
            // To see if the sign-in was successful, the user should have an access_token.
            app.CurrentPage.Text.Should().Contain("Bearer ey");
            
            // Test ASP.NET Core authorization pipeline
            await app.GoTo("/custom/me");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain(subJohnDoe);
        }
        finally
        {
            await app.CloseBrowser();
            await Task.Delay(1000);
        }
    }
    
    [Fact]
    public async Task WhenNotAuthenticated_SlashDotAuthSlashMe_ShouldReturn401()
    {
        var app = new App();

        try
        {
            const string path = "/.auth/me";
            
            // Navigate to the cho endpoint
            await app.NavigateToProxy(path);
            await Task.Delay(2500);

            app.CurrentStatus.Should().Be(HttpStatusCode.Unauthorized);
        }
        finally
        {
            await app.CloseBrowser();
            await Task.Delay(1000);
        }
    }

    public async Task DisposeAsync()
    {
        await _testApi?.StopAsync()!;
    }
}