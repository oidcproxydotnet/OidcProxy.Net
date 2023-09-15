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

        var username = configuration.GetSection("Auth0:Username").Get<string>();
        var password = configuration.GetSection("Auth0:Password").Get<string>();
        
        var app = new App();

        try
        {
            await app.NavigateToBff();
            await Task.Delay(1000);

            await app.Auth0LoginPage.TxtUsername.TypeAsync(username);
            await app.Auth0LoginPage.TxtPassword.TypeAsync(password);
            await app.Auth0LoginPage.BtnContinue.ClickAsync();
            
            await app.WaitForNavigationAsync();

            app.MeEndpoint.Text.Should().Contain(username);
        
            // Assert the user was logged in
            await app.GoTo("/api/echo");
            await Task.Delay(1000);

            app.EchoEndpoint.Text.Should().Contain("Bearer ey");

            // Log out
            await app.GoTo("/account/end-session");
            await Task.Delay(1000);
            
            await app.Auth0SignOutPage.BtnAccept.ClickAsync();
            await app.WaitForNavigationAsync();
            
            // Assert user details flushed
            await app.GoTo("/account/me");
            await Task.Delay(1000);
            
            app.MeEndpoint.Text.Should().NotContain(username);
            
            // Assert token removed
            await app.GoTo("/api/echo");
            await Task.Delay(1000);
            
            app.EchoEndpoint.Text.Should().NotContain("Bearer ey");
        }
        finally
        {
            await app.CloseBrowser();
        }
    }
}