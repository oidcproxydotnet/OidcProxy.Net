using FluentAssertions;
using Host.TestApps.Auth0.IntegrationTests.Pom;
using Microsoft.Extensions.Configuration;

namespace Host.TestApps.Auth0.IntegrationTests;

public class Auth0IntegrationTests : IClassFixture<HostApplication>
{
    [Fact]
    public async Task ShouldIntegrateWithAuth0()
    {
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(GetType().Assembly)
            .AddEnvironmentVariables()
            .Build();

        var sub = configuration.GetSection("Auth0:Sub").Get<string>();
        var username = configuration.GetSection("Auth0:Username").Get<string>();
        var password = configuration.GetSection("Auth0:Password").Get<string>();
        
        var app = new App();

        try
        {
            await app.NavigateToProxy();
            await Task.Delay(1000);

            await app.Auth0LoginPage.TxtUsername.TypeAsync(username);
            await app.Auth0LoginPage.HitEnter();
            await Task.Delay(1000);
            
            await app.Auth0LoginPage.TxtPassword.TypeAsync(password);
            await app.Auth0LoginPage.HitEnter();
            
            await app.WaitForNavigationAsync();

            app.CurrentPage.Text.Should().Contain(username);
        
            // Assert the user was logged in
            await app.GoTo("/api/echo");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain("Bearer ey");
            
            // Test ASP.NET Core authorization pipeline
            await app.GoTo("/custom/me");
            await Task.Delay(1000);

            app.CurrentPage.Text.Should().Contain(sub);

            // Log out
            await app.GoTo("/.auth/end-session");
            await Task.Delay(1000);
            
            // Assert user details flushed
            await app.GoTo("/.auth/me");
            await Task.Delay(1000);
            
            app.CurrentPage.Text.Should().NotContain(username);
            
            // Assert token removed
            await app.GoTo("/api/echo");
            await Task.Delay(1000);
            
            app.CurrentPage.Text.Should().NotContain("Bearer ey");
        }
        finally
        {
            await app.CloseBrowser();
        }
    }
}