using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoCloudNative.Bff.Authentication;

public static class LoginEndpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app, string endpointName)
    {
        app.Map($"/{endpointName}/me", (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            if (!context.Session.HasIdToken())
            {
                return Results.NotFound();
            }

            var idToken = context.Session.GetIdToken();
            return Results.Ok(idToken.ParseJwtPayload());
        });
        
        app.Map($"/{endpointName}/login", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            var redirectUri = CreateRedirectUri(context, endpointName);
            
            var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

            if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
            {
                context.Session.SetCodeVerifier(authorizeRequest.CodeVerifier);
            }

            context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
        });

        app.Map($"/{endpointName}/login/callback", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            var code = context.Request.Query["code"].SingleOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("The querystring parameter 'code' cannot be empty. Invoke the /login endpoint first.");
            }
            
            var redirectUrl = CreateRedirectUri(context, endpointName);

            var codeVerifier = context.Session.GetCodeVerifier(); 
            var tokenResponse = await identityProvider.GetTokenAsync(redirectUrl, code, codeVerifier);
            
            context.Session.RemoveCodeVerifier();

            context.Session.Save(tokenResponse);

            context.Response.Redirect("/");

        });
        
        app.Map($"/{endpointName}/revoke", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            if (context.Session.HasAccessToken())
            {
                var accessToken = context.Session.GetAccessToken();
                await identityProvider.Revoke(accessToken);
            }
            
            if (context.Session.HasRefreshToken())
            {
                var refreshToken = context.Session.GetRefreshToken();
                await identityProvider.Revoke(refreshToken);
            }
            
            context.Session.Clear();
        });
    }

    private static string CreateRedirectUri(HttpContext context, string endpointName)
    {
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{context.Request.Host}/{endpointName}/login/callback";
        return redirectUrl;
    }
}