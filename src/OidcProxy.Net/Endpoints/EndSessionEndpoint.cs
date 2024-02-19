using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class EndSessionEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] ILogger<IIdentityProvider> logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] IIdentityProvider identityProvider)
    {
        
        if (!context.Session.HasAccessToken())
        {
            return Results.BadRequest();
        }

        logger.LogLine(context, $"Revoking access_token.");
        var accessToken = context.Session.GetAccessToken();

        try
        {
            await identityProvider.RevokeAsync(accessToken!, context.TraceIdentifier);
        }
        catch (ApplicationException e)
        {
            logger.Warn(context.TraceIdentifier, $"Unexpected: Failed to revoke access_token during end-session. " +
                                                 $"Access_token will be removed from the OidcProxy.Net http-session. " +
                                                 $"This event is only visible in the logs. " +
                                                 $"The following error occured: {e}");
        }
        
        logger.LogLine(context, "Revoking refresh_token.");
        var refreshToken = context.Session.GetRefreshToken();

        try
        {
            await identityProvider.RevokeAsync(refreshToken!, context.TraceIdentifier);
        }
        catch (ApplicationException e)
        {
            logger.Warn(context.TraceIdentifier, $"Unexpected: Failed to revoke refresh_token during end-session. " +
                                                 $"Refresh_token will be removed from the OidcProxy.Net http-session. " +
                                                 $"This event is only visible in the logs. " +
                                                 $"The following error occured: {e}");
        }

        string? idToken = null;
        if (context.Session.HasIdToken())
        {
            idToken = context.Session.GetIdToken();
        }

        context.Session.Clear();

        var baseAddress = $"{redirectUriFactory.DetermineHostName(context)}";

        var endSessionEndpoint = await identityProvider.GetEndSessionEndpointAsync(idToken, baseAddress);

        logger.LogLine(context, $"Redirect to {endSessionEndpoint}");

        return Results.Redirect(endSessionEndpoint.ToString());
    }
}