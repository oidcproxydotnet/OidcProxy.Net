using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net;

namespace Host.TestApps.Auth0;

public class TestAuthenticationCallbackHandler : DefaultAuthenticationCallbackHandler
{
    public TestAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger) : base(logger)
    {
    }

    public override Task<IResult> OnAuthenticated(HttpContext context, 
        JwtPayload? payload,
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        var customLandingPageWorks = string.IsNullOrEmpty(userPreferredLandingPage)
            ? Results.Redirect($"{defaultLandingPage}?custom_landing_page_works")
            : Results.Redirect(userPreferredLandingPage);

        return Task.FromResult(customLandingPageWorks);
    }
}