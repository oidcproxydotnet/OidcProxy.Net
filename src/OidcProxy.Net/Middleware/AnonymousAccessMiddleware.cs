using Microsoft.AspNetCore.Http;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.Middleware;

internal class AnonymousAccessMiddleware(
    EndpointName oidcProxyReservedEndpointName,
    IAuthSession authSession,
    ILogger logger,
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

        var authorizeRequest = await authSession.InitiateAuthenticationSequence(currentPath);
        
        await logger.InformAsync($"Redirect({authorizeRequest.AuthorizeUri})");
        
        context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
    }
}