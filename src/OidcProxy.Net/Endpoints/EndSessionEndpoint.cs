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
        try
        {
            if (context.Session.HasAccessToken())
            {
                var accessToken = context.Session.GetAccessToken();

                logger.LogLine(context, $"Revoking access_token.");
                await identityProvider.RevokeAsync(accessToken!, context.TraceIdentifier);
            }

            if (context.Session.HasRefreshToken())
            {
                var refreshToken = context.Session.GetRefreshToken();

                logger.LogLine(context, $"Revoking refresh_token.");
                await identityProvider.RevokeAsync(refreshToken!, context.TraceIdentifier);
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
        catch (Exception e)
        {
            logger.LogException(context, e);
            throw;
        }
    }
}