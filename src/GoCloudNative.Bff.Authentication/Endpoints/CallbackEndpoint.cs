using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal static class CallbackEndpoint<TIdp> where TIdp : IIdentityProvider
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] ILogger<TIdp> logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] BffOptions bffOptions,
        [FromServices] TIdp identityProvider,
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

            var codeVerifier = context.Session.GetCodeVerifier<TIdp>();

            logger.LogLine(context, "Exchanging code for access_token.");
            var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier, context.TraceIdentifier);

            await context.Session.RemoveCodeVerifierAsync<TIdp>();

            await context.Session.SaveAsync<TIdp>(tokenResponse);

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