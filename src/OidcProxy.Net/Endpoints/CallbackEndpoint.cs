using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class CallbackEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] ILogger<IIdentityProvider> logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] BffOptions bffOptions,
        [FromServices] IIdentityProvider identityProvider,
        [FromServices] IAuthenticationCallbackHandler callbackHandler)
    {
        try
        {
            var code = context.Request.Query["code"].SingleOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                logger.LogLine(context, "Unable to obtain access token. Querystring parameter 'code' has no value.");
                
                var redirectUri = $"{bffOptions.ErrorPage}{context.Request.QueryString}";
                return await callbackHandler.OnAuthenticationFailed(context, redirectUri);
            }
            
            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login/callback");
            var redirectUrl = redirectUriFactory.DetermineRedirectUri(context, endpointName);

            var codeVerifier = context.Session.GetCodeVerifier();

            logger.LogLine(context, "Exchanging code for access_token.");
            var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier, context.TraceIdentifier);

            await context.Session.RemoveCodeVerifierAsync();

            await context.Session.SaveAsync(tokenResponse);

            logger.LogLine(context, $"Redirect({bffOptions.LandingPage})");

            return await callbackHandler.OnAuthenticated(context, bffOptions.LandingPage.ToString());
        }
        catch (Exception e)
        {
            logger.LogException(context, e);
            await callbackHandler.OnError(context, e);
            throw;
        }
    }
}