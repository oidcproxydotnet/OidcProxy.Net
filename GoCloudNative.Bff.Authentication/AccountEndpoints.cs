using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication;

internal static class AccountEndpoints
{
    public static void MapAuthenticationEndpoints<TIdp>(this WebApplication app, string endpointName) where TIdp : IIdentityProvider
    {
        app.Map($"/{endpointName}/me", async (HttpContext context, 
            [FromServices] TIdp identityProvider, 
            [FromServices] IClaimsTransformation claimsTransformation) =>
        {   
            if (!context.Session.HasIdToken<TIdp>())
            {
                return Results.NotFound();
            }
            
            context.Response.Headers.CacheControl = $"no-cache, no-store, must-revalidate";

            var idToken = context.Session.GetIdToken<TIdp>();
            var payload = idToken.ParseJwtPayload();
            var claims = await claimsTransformation.Transform(payload);
            return Results.Ok(claims);
        });
        
        app.Map($"/{endpointName}/login", async (HttpContext context, 
            [FromServices] ILogger<TIdp> logger, 
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] TIdp identityProvider) =>
        {
            try
            {            
                var redirectUri = redirectUriFactory.DetermineRedirectUri(context, endpointName);
            
                var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

                if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
                {
                    await context.Session.SetCodeVerifierAsync<TIdp>(authorizeRequest.CodeVerifier);
                }

                logger.LogLine(context, new LogLine($"Redirect({authorizeRequest.AuthorizeUri})"));
            
                context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
            }
            catch (Exception e)
            {
                logger.LogException(context, e);
                throw;
            }
        });

        app.Map($"/{endpointName}/login/callback", async (HttpContext context, 
            [FromServices] ILogger<TIdp> logger, 
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] BffOptions bffOptions,
            [FromServices] TIdp identityProvider) =>
        {
            try
            {
                var code = context.Request.Query["code"].SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    logger.LogLine(context, new LogLine($"Unable to obtain access token. Querystring parameter 'code' has no value."));

                    var redirectUri = $"{bffOptions.ErrorPage}{context.Request.QueryString}";
                    logger.LogLine(context, new LogLine($"Redirect({redirectUri})"));
                    
                    return Results.Redirect(redirectUri);
                }
            
                var redirectUrl = redirectUriFactory.DetermineRedirectUri(context, endpointName);

                var codeVerifier = context.Session.GetCodeVerifier<TIdp>(); 
                
                logger.LogLine(context, new LogLine($"Exchanging code for access_token."));
                var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier);
            
                await context.Session.RemoveCodeVerifierAsync<TIdp>();

                await context.Session.SaveAsync<TIdp>(tokenResponse);
                
                logger.LogLine(context, new LogLine($"Redirect({bffOptions.LandingPage})"));
                
                return Results.Redirect(bffOptions.LandingPage.ToString());
            }
            catch (Exception e)
            {
                logger.LogException(context, e);
                throw;
            }
        });
        
        app.MapGet($"/{endpointName}/login/callback/error", () => Results.Text("Login failed."));
        
        app.MapGet($"/{endpointName}/end-session", async (HttpContext context, 
            [FromServices] ILogger<TIdp> logger,
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] TIdp identityProvider) =>
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
        });

    }
}