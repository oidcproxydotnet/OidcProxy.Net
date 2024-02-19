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

public class JwtTest : IAsyncLifetime, 
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
    public async Task ItShouldWork()
    {
        const string subJohnDoe = "johndoe";
        
        var app = new App();

        try
        {
            await app.NavigateToProxy();

            app.CurrentPage.Text.Should().Contain(subJohnDoe);
        
            // Assert the user was logged in
            await app.GoTo("/.auth/me");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain(subJohnDoe);
            
            // Test ASP.NET Core authorization pipeline
            await app.GoTo("/custom/me");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain(subJohnDoe);

            // Assert the user was logged in
            await app.GoTo("/api/echo");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain("Bearer ey");
            
            // Sign out
            await app.GoTo("/.auth/end-session");
            await Task.Delay(1000);
            
            // Assert user details flushed
            await app.GoTo("/.auth/me");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().NotContain(subJohnDoe);
            
            // Assert token removed
            await app.GoTo("/api/echo");
            await Task.Delay(1000);
        
            app.CurrentPage.Text.Should().NotContain("Bearer ey");
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