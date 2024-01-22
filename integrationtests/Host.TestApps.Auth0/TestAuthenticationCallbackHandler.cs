using OidcProxy.Net;
using OidcProxy.Net.Endpoints;

namespace Host.TestApps.Auth0;

public class TestAuthenticationCallbackHandler : DefaultAuthenticationCallbackHandler
{
    public TestAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger) : base(logger)
    {
    }

    public override Task<IResult> OnAuthenticated(HttpContext context, 
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        return base.OnAuthenticated(context, $"{defaultLandingPage}?custom_callback_handler_works=true", null);
    }
}