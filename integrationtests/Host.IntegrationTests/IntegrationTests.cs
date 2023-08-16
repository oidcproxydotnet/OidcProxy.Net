using FluentAssertions;
using Host.IntegrationTests.Fixtures;
using Host.IntegrationTests.Pom;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Host.IntegrationTests;

public class IntegrationTests : IClassFixture<HostApplication>
{
    [Fact]
    public async Task ShouldIntegrateWithIdentityServer4()
    {
        const string subYoda = "test-user-2";
        const string protocol = "oidc";

        const string endSessionEndpoint = "/oidc/end-session";
        
        var app = new App();
        await app.NavigateToBff();
        
        try
        {
            // Login
            await app.HomePage.BtnOidcLogin.ClickAsync();
            await app.WaitForNavigationAsync();
            await app.IdSvrLoginPage.BtnYodaLogin.ClickAsync();
            await app.WaitForNavigationAsync();

            // Assert token is being forwarded to downstream apis
            await app.GoTo(EchoEndpoint.Uri);
            await Task.Delay(1000);
        
            app.EchoEndpoint.Text.Should().Contain("Bearer ey");
        
            // Assert the user was logged in
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);

            app.MeEndpoint.Text.Should().Contain(subYoda);

            // Log out
            await app.GoTo(endSessionEndpoint);
            await Task.Delay(1000);
        
            await app.IdSvrSignOutPage.BtnYes.ClickAsync();
            await app.WaitForNavigationAsync();
        
            // Assert user details flushed
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);

            app.MeEndpoint.Text.Should().NotContain(subYoda);
            
            // Assert token removed
            await app.GoTo(EchoEndpoint.Uri);
            await Task.Delay(1000);
        
            app.EchoEndpoint.Text.Should().NotContain("Bearer ey");
        }
        finally
        {
            await app.CloseBrowser();
        }
    }
    
    [Fact]
    public async Task ShouldIntegrateWithAuth0()
    {
        const string protocol = "auth0";

        const string endSessionEndpoint = "/auth0/end-session";
        
        var configuration = new ConfigurationBuilder()
            .AddUserSecrets(GetType().Assembly)
            .Build();

        var username = configuration.GetSection("Auth0:Username").Get<string>();
        var password = configuration.GetSection("Auth0:Password").Get<string>();
        
        var app = new App();
        await app.NavigateToBff();
        
        try
        {
            // Login
            await app.HomePage.BtnAuth0Login.ClickAsync();
            await app.WaitForNavigationAsync();

            await app.Auth0LoginPage.TxtUsername.TypeAsync(username);
            await app.Auth0LoginPage.TxtPassword.TypeAsync(password);
            await app.Auth0LoginPage.BtnContinue.ClickAsync();
            
            await app.WaitForNavigationAsync();

            // Assert token is being forwarded to downstream apis
            await app.GoTo(EchoEndpoint.Uri);
            await Task.Delay(1000);
            
            app.EchoEndpoint.Text.Should().Contain("Bearer ey");
            
            // Assert the user was logged in
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);
            
            app.MeEndpoint.Text.Should().Contain(username);
            
            // Log out
            await app.GoTo(endSessionEndpoint);
            await Task.Delay(1000);
            
            await app.Auth0SignOutPage.BtnAccept.ClickAsync();
            await app.WaitForNavigationAsync();
            
            // Assert user details flushed
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);

            app.MeEndpoint.Text.Should().NotContain(username);

            // Assert token removed
            await app.GoTo(EchoEndpoint.Uri);
            await Task.Delay(1000);
        
            app.EchoEndpoint.Text.Should().NotContain("Bearer ey");
        }
        finally
        {
            await app.CloseBrowser();
        }
    }
}