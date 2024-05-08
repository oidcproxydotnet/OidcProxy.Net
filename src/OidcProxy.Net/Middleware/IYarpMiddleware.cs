using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Middleware;

public interface IYarpMiddleware
{
    Task Apply(HttpContext context, Func<HttpContext, Task> next);
}