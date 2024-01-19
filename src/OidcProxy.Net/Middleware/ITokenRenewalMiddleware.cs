using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Middleware;

internal interface ITokenRenewalMiddleware
{
    Task Apply(HttpContext context, Func<HttpContext, Task> next);
}