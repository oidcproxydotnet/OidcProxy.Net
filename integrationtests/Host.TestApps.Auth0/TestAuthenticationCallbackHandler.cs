using OidcProxy.Net.Endpoints;

namespace Host.TestApps.Auth0;

public class TestAuthenticationCallbackHandler : DefaultAuthenticationCallbackHandler
{
    public TestAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger) : base(logger)
    {
    }

    public override Task<IResult> OnAuthenticated(HttpContext context, string defaultRedirectUrl)
    {
        return base.OnAuthenticated(context, $"{defaultRedirectUrl}?custom_callback_handler_works=true");
    }
}