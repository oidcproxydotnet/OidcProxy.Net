using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Middleware;

internal class AnonymousAccessMiddleware(
    EndpointName oidcProxyReservedEndpointName,
    AuthSession authSession,
    ILogger logger,
    IRedirectUriFactory redirectUriFactory,
    IIdentityProvider identityProvider,
    IHttpContextAccessor httpContextAccessor)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var currentPath = context.Request.Path + context.Request.QueryString;
        if (currentPath.StartsWith(oidcProxyReservedEndpointName.ToString(), StringComparison.InvariantCultureIgnoreCase))
        {
            await next(context);
            return;
        }

        var session = httpContextAccessor.HttpContext?.Session;
        if (session?.GetAccessToken() != null)
        {
            await next(context);
            return;
        }

        await authSession.SetUserPreferredLandingPageAsync(currentPath);
        
        var redirectUri = redirectUriFactory.DetermineRedirectUri(context, oidcProxyReservedEndpointName.ToString());
        
        var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);
        
        if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
        {
            await authSession.SetCodeVerifierAsync(authorizeRequest.CodeVerifier);
        }
        
        await logger.InformAsync($"Redirect({authorizeRequest.AuthorizeUri})");
        
        context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
    }
}