using FluentAssertions;
using Host.TestApps.IntegrationTests.Fixtures;
using Host.TestApps.IntegrationTests.Fixtures.OpenIddict;
using Host.TestApps.IntegrationTests.Pom;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.OpenIdConnect.Jwe;
using Xunit;

namespace Host.TestApps.IntegrationTests;

public class JweSymmetricSecurityKeyTest : IAsyncLifetime, 
    IClassFixture<EchoFixture>, // Run an echo-api
    IClassFixture<OpenIddictFixture> // Run an oidc-server
{
    private WebApplication? _testApi = null;
    
    public async Task InitializeAsync()
    {
        OidcProxy.Net.ModuleInitializers.ModuleInitializer.Reset();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();
        
        var key = new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="));
        
        builder.Services.AddOidcProxy(config, o => o.UseJweKey(new EncryptionKey(key)));

        _testApi = builder.Build();
        
        _testApi
            .MapGet("/custom/me", async context => await context.Response.WriteAsJsonAsync(context.User.Identity?.Name))
            .RequireAuthorization();
        
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