using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Middleware;

internal interface IYarpMiddleware
{
    Task Apply(HttpContext context, Func<HttpContext, Task> next);
}