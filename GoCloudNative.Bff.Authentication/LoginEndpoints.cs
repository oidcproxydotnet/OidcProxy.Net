using System.Text;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoCloudNative.Bff.Authentication;

public static class LoginEndpoints
{
    private static readonly string VerifierKey = "verifier_key";

    private static readonly string TokenKey = "token_key";

    private static readonly string IdTokenKey = "id_token_key";
    
    private static readonly string RefreshTokenKey = "refresh_token_key";

    public static void MapAuthenticationEndpoints(this WebApplication app)
    {   
        app.Map("/oidc/login", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(context);

            if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
            {
                context.Session.SetString(VerifierKey, authorizeRequest.CodeVerifier);
            }

            context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
        });

        app.Map("/oidc/login/callback", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            var code = context.Request.Query["code"].SingleOrDefault();
            if (string.IsNullOrEmpty(code))
            {
                throw new ArgumentException("The querystring parameter 'code' cannot be empty. Invoke the /login endpoint first.");
            }
            
            var codeVerifier = context.Session.GetString(VerifierKey); 
            var tokenResponse = await identityProvider.GetTokenAsync(context, code, codeVerifier);
            
            context.Session.Remove(VerifierKey);
            
            Set(TokenKey, tokenResponse.access_token);
            Set(IdTokenKey, tokenResponse.id_token);
            Set(RefreshTokenKey, tokenResponse.refresh_token);
            
            context.Response.Redirect("/");

            void Set(string key, string? value)
            {
                if (value == null && context.Session.Keys.Contains(key))
                {
                    context.Session.Remove(key);
                }

                if (value != null)
                {
                    context.Session.SetString(key, value);
                }
            }
        });
        
        app.Map("/oidc/revoke", async (HttpContext context, [FromServices] IIdentityProvider identityProvider) =>
        {
            context.Session.Remove(IdTokenKey);
            
            if (context.Session.TryGetValue(TokenKey, out var accessTokenBytes))
            {
                var accessToken = Encoding.UTF8.GetString(accessTokenBytes);
                await identityProvider.Revoke(accessToken);
                context.Session.Remove(TokenKey);
            }

            if (context.Session.TryGetValue(RefreshTokenKey, out var refreshTokenBytes))
            {
                var refreshToken = Encoding.UTF8.GetString(refreshTokenBytes);
                await identityProvider.Revoke(refreshToken);
                context.Session.Remove(RefreshTokenKey);
            }
        });
    }
}