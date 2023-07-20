using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal static class EndSessionEndpoint<TIdp>
    where TIdp : IIdentityProvider
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] ILogger<TIdp> logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] TIdp identityProvider)
    {
        try
        {
            if (context.Session.HasAccessToken<TIdp>())
            {
                var accessToken = context.Session.GetAccessToken<TIdp>();

                logger.LogLine(context, new LogLine($"Revoking access_token."));
                await identityProvider.RevokeAsync(accessToken);
            }

            if (context.Session.HasRefreshToken<TIdp>())
            {
                var refreshToken = context.Session.GetRefreshToken<TIdp>();

                logger.LogLine(context, new LogLine($"Revoking refresh_token."));
                await identityProvider.RevokeAsync(refreshToken);
            }

            string? idToken = null;
            if (context.Session.HasIdToken<TIdp>())
            {
                idToken = context.Session.GetIdToken<TIdp>();
            }

            context.Session.Clear();

            var baseAddress = $"{redirectUriFactory.DetermineHostName(context)}";

            var endSessionEndpoint = await identityProvider.GetEndSessionEndpointAsync(idToken, baseAddress);

            logger.LogLine(context, new LogLine($"Redirect to {endSessionEndpoint}"));

            return Results.Redirect(endSessionEndpoint.ToString());
        }
        catch (Exception e)
        {
            logger.LogException(context, e);
            throw;
        }
    }
}