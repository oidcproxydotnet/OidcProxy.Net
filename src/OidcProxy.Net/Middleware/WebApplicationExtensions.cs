using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.Middleware;

internal static class WebApplicationExtensions
{
    public static void RegisterYarpMiddleware(this WebApplication app, IEnumerable<Type> types)
    {
        if (!types.Any())
        {
            return;
        }

        app.MapReverseProxy(x => x.Use((context, next) =>
        {
            var delegates = new List<Func<HttpContext, Task>> { _ => next() };

            foreach (var type in types)
            {
                var instance = (IYarpMiddleware) x.ApplicationServices.GetRequiredService(type);
                var previous = delegates.Last();
                delegates.Add(ctx => instance.Apply(ctx, previous));
            }

            return delegates.Last()(context);
        }));
    }
}