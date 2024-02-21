using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net;
using ILogger = OidcProxy.Net.Logging.ILogger;

namespace Host.TestApps.Auth0;

public class TestAuthenticationCallbackHandler : DefaultAuthenticationCallbackHandler
{
    public TestAuthenticationCallbackHandler(ILogger logger) : base(logger)
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