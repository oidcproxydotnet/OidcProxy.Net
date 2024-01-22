using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class LoginEndpoint
{
    public static async Task Get(HttpContext context,
            [FromServices] ILogger<IIdentityProvider> logger,
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] IIdentityProvider identityProvider)
    {
        try
        {
            await context.Session.SetUserPreferredLandingPageAsync(context.Request.Query["landingpage"]);
            
            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login");
            
            var redirectUri = redirectUriFactory.DetermineRedirectUri(context, endpointName);

            var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

            if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
            {
                await context.Session.SetCodeVerifierAsync(authorizeRequest.CodeVerifier);
            }

            logger.LogLine(context, $"Redirect({authorizeRequest.AuthorizeUri})");

            context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
        }
        catch (Exception e)
        {
            logger.LogException(context, e);
            throw;
        }
    }
}