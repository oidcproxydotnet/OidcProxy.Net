using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Logging;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class CallbackEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] AuthSession authSession,
        [FromServices] ILogger logger,
        [FromServices] IRedirectUriFactory redirectUriFactory,
        [FromServices] ProxyOptions proxyOptions,
        [FromServices] IIdentityProvider identityProvider,
        [FromServices] ITokenParser tokenParser,
        [FromServices] IJwtSignatureValidator jwtSignatureValidator,
        [FromServices] IAuthenticationCallbackHandler authenticationCallbackHandler)
    {
        try
        {
            var userPreferredLandingPage = authSession.GetUserPreferredLandingPage();

            var code = context.Request.Query["code"].SingleOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                await logger.InformAsync("Unable to obtain access token. Querystring parameter 'code' has no value.");

                var redirectUri = $"{proxyOptions.ErrorPage}{context.Request.QueryString}";
                return await authenticationCallbackHandler.OnAuthenticationFailed(context, redirectUri, userPreferredLandingPage);
            }

            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login/callback");
            var redirectUrl = redirectUriFactory.DetermineRedirectUri(endpointName);

            var codeVerifier = authSession.GetCodeVerifier();

            await logger.InformAsync("Exchanging code for access_token.");
            var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier, context.TraceIdentifier);

            if (!(await jwtSignatureValidator.Validate(tokenResponse.access_token)))
            {
                return await authenticationCallbackHandler.OnAuthenticationFailed(context,
                    proxyOptions.LandingPage.ToString(),
                    userPreferredLandingPage);
            }

            await authSession.SaveAsync(tokenResponse);

            await logger.InformAsync($"Redirect({proxyOptions.LandingPage})");

            var jwtPayload = tokenParser.ParseJwtPayload(tokenResponse.access_token);

            return await authenticationCallbackHandler.OnAuthenticated(context,
                jwtPayload,
                proxyOptions.LandingPage.ToString(),
                userPreferredLandingPage);
        }
        catch (Exception e)
        {
            await logger.ErrorAsync(e);
            await authenticationCallbackHandler.OnError(context, e);
            throw;
        }
        finally
        {
            await authSession.RemoveCodeVerifierAsync();
        }
    }
}