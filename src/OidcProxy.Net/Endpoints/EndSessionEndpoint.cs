using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class EndSessionEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] AuthSession authSession,
        [FromServices] ILogger logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] IIdentityProvider identityProvider)
    {
        
        if (!authSession.HasAccessToken())
        {
            return Results.BadRequest();
        }

        await logger.InformAsync("Revoking access_token.");
        var accessToken = context.Session.GetAccessToken();

        try
        {
            await identityProvider.RevokeAsync(accessToken!, context.TraceIdentifier);
        }
        catch (Exception e) when (e is ApplicationException || e is ArgumentException)
        {
            await logger.WarnAsync($"Unexpected: Failed to revoke access_token during end-session. " +
                              $"Access_token will be removed from the OidcProxy.Net http-session. " +
                              $"This event is only visible in the logs. " +
                              $"The following error occured: {e}");
        }
        
        await logger.InformAsync("Revoking refresh_token.");
        var refreshToken = authSession.GetRefreshToken();

        try
        {
            await identityProvider.RevokeAsync(refreshToken!, context.TraceIdentifier);
        }
        catch (Exception e) when (e is ApplicationException || e is ArgumentException)
        {
            await logger.WarnAsync($"Unexpected: Failed to revoke refresh_token during end-session. " +
                              $"Refresh_token will be removed from the OidcProxy.Net http-session. " +
                              $"This event is only visible in the logs. " +
                              $"The following error occured: {e}");
        }

        string? idToken = null;
        if (authSession.HasIdToken())
        {
            idToken = authSession.GetIdToken();
        }

        context.Session.Clear();

        var baseAddress = $"{redirectUriFactory.DetermineHostName(context)}";

        var endSessionEndpoint = await identityProvider.GetEndSessionEndpointAsync(idToken, baseAddress);

        await logger.InformAsync($"Redirect to {endSessionEndpoint}");

        return Results.Redirect(endSessionEndpoint.ToString());
    }
}