using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Logging;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication;

public static class AccountEndpoints
{
    public static void MapAuthenticationEndpoints<T>(this WebApplication app, string endpointName) where T : IIdentityProvider
    {
        app.Map($"/{endpointName}/me", (HttpContext context, [FromServices] T identityProvider) =>
        {   
            if (!context.Session.HasIdToken())
            {
                return Results.NotFound();
            }
            
            context.Response.Headers.CacheControl = $"no-cache, no-store, must-revalidate";

            var idToken = context.Session.GetIdToken();
            return Results.Ok(idToken.ParseJwtPayload());
        });
        
        app.Map($"/{endpointName}/login", async (HttpContext context, 
            [FromServices] ILogger<Endpoints> logger, 
            [FromServices] T identityProvider) =>
        {
            try
            {            
                var redirectUri = DetermineRedirectUri(context, endpointName);
            
                var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

                if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
                {
                    context.Session.SetCodeVerifier(authorizeRequest.CodeVerifier);
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
            [FromServices] ILogger<Endpoints> logger, 
            [FromServices] T identityProvider) =>
        {
            try
            {
                var code = context.Request.Query["code"].SingleOrDefault();
                if (string.IsNullOrEmpty(code))
                {
                    logger.LogLine(context, new LogLine($"BadRequest (querystring parameter 'code' has is required)"));
                    return Results.BadRequest();
                }
            
                var redirectUrl = DetermineRedirectUri(context, endpointName);

                var codeVerifier = context.Session.GetCodeVerifier(); 
                
                logger.LogLine(context, new LogLine($"Exchanging code for access_token."));
                var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier);
            
                context.Session.RemoveCodeVerifier();

                context.Session.Save(tokenResponse);

                logger.LogLine(context, new LogLine($"Redirect(/)"));
                
                return Results.Redirect("/");
            }
            catch (Exception e)
            {
                logger.LogException(context, e);
                throw;
            }
        });
        
        app.MapGet($"/{endpointName}/end-session", async (HttpContext context, 
            [FromServices] ILogger<Endpoints> logger,
            [FromServices] T identityProvider) =>
        {
            try
            {
                if (context.Session.HasAccessToken())
                {
                    var accessToken = context.Session.GetAccessToken();
                    
                    logger.LogLine(context, new LogLine($"Revoking access_token."));
                    await identityProvider.Revoke(accessToken);
                }
            
                if (context.Session.HasRefreshToken())
                {
                    var refreshToken = context.Session.GetRefreshToken();
                    
                    logger.LogLine(context, new LogLine($"Revoking refresh_token."));
                    await identityProvider.Revoke(refreshToken);
                }

                string? idToken = null;
                if (context.Session.HasIdToken())
                {
                    idToken = context.Session.GetIdToken();
                }
            
                context.Session.Clear();

                var baseAddress = $"{DetermineHostName(context)}";
                
                var endSessionEndpoint = await identityProvider.GetEndSessionEndpoint(idToken, baseAddress);
                
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

    private static string DetermineHostName(HttpContext context)
    {
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        return $"{protocol}{context.Request.Host}";
    }

    private static string DetermineRedirectUri(HttpContext context, string endpointName)
    {
        var hostName = DetermineHostName(context);
        return $"{hostName}/{endpointName}/login/callback";
    }
}

public class Endpoints
{
}