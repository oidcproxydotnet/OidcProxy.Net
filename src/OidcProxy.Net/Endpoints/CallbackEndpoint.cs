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
        [FromServices] ProxyOptions proxyOptions,
        [FromServices] IIdentityProvider identityProvider,
        [FromServices] ITokenParser tokenParser,
        [FromServices] IAuthenticationCallbackHandler authenticationCallbackHandler)
    {
        try
        {
            var userPreferredLandingPage = GetUserPreferredLandingPage(context);
            
            var code = context.Request.Query["code"].SingleOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                logger.LogLine(context, "Unable to obtain access token. Querystring parameter 'code' has no value.");
                
                var redirectUri = $"{proxyOptions.ErrorPage}{context.Request.QueryString}";
                return await authenticationCallbackHandler.OnAuthenticationFailed(context, redirectUri, userPreferredLandingPage);
            }
            
            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login/callback");
            var redirectUrl = redirectUriFactory.DetermineRedirectUri(context, endpointName);

            var codeVerifier = context.Session.GetCodeVerifier();

            logger.LogLine(context, "Exchanging code for access_token.");
            var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier, context.TraceIdentifier);

            await context.Session.RemoveCodeVerifierAsync();

            await context.Session.SaveAsync(tokenResponse);

            logger.LogLine(context, $"Redirect({proxyOptions.LandingPage})");

            var jwtPayload = tokenParser.ParseAccessToken(tokenResponse.access_token);
            
            return await authenticationCallbackHandler.OnAuthenticated(context, 
                jwtPayload, 
                proxyOptions.LandingPage.ToString(), 
                userPreferredLandingPage);
        }
        catch (Exception e)
        {
            logger.LogException(context, e);
            await authenticationCallbackHandler.OnError(context, e);
            throw;
        }
    }

    private static string? GetUserPreferredLandingPage(HttpContext context)
    {
        return context.Session.GetUserPreferredLandingPage();
    }
}