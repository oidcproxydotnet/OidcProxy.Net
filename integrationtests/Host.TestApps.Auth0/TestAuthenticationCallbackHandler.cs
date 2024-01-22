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
        return base.OnAuthenticated(context, payload, $"{defaultLandingPage}?custom_callback_handler_works=true", null);
    }
}