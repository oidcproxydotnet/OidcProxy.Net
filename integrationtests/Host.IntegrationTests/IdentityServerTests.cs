using FluentAssertions;
using Host.IntegrationTests.Fixtures;
using Host.IntegrationTests.Pom;
using Xunit;

namespace Host.IntegrationTests;

public class IdentityServerTests : IClassFixture<HostFixture>
{
    [Fact]
    public async Task ItShouldWork()
    {
        const string subYoda = "test-user-2";
        const string protocol = "oidc";
        
        var app = new App();
        await app.NavigateToBff();
        
        try
        {
            // Login
            await app.HomePage.BtnOidcLogin.ClickAsync();
            await app.WaitForNavigationAsync();
            await app.IdSvrLoginPage.BtnYodaLogin.ClickAsync();
            await app.WaitForNavigationAsync();

            // Assert token is being forwarded to downstream api's
            await app.GoTo(EchoEndpoint.Uri);
            await Task.Delay(1000);
        
            app.EchoEndpoint.Text.Should().MatchRegex(@"Bearer\ ey");
        
            // Assert the user was logged in
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);

            app.MeEndpoint.Text.Should().Contain(subYoda); // yoda

            // Log out
            await app.GoTo("/oidc/end-session");
            await Task.Delay(1000);
        
            await app.IdSvrSignOutPage.BtnYes.ClickAsync();
            await app.WaitForNavigationAsync();
        
            await app.GoTo(MeEndpoint.GetUri(protocol));
            await Task.Delay(1000);

            app.MeEndpoint.Text.Should().NotContain(subYoda); // yoda
        }
        finally
        {
            await app.CloseBrowser();
        }
    }
}