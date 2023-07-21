using System.Text.RegularExpressions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal static class LoginEndpoint<TIdp> where TIdp : IIdentityProvider
{
    public static async Task Get(HttpContext context,
            [FromServices] ILogger<TIdp> logger,
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] TIdp identityProvider)
    {
        try
        {
            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login");
            
            var redirectUri = redirectUriFactory.DetermineRedirectUri(context, endpointName);

            var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

            if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
            {
                await context.Session.SetCodeVerifierAsync<TIdp>(authorizeRequest.CodeVerifier);
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