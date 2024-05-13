using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Middleware;

/// <summary>
/// This interface must be implemented by custom middleware that is added into the YARP pipeline
/// by <see cref="ModuleInitializers.ProxyOptions.AddYarpMiddleware"/>.
/// </summary>
public interface IYarpMiddleware
{
    /// <summary>
    /// Implementations of this method should perform their custom processing using the provided context
    /// and then await next(context) to continue pipeline processing.
    /// </summary>
    Task Apply(HttpContext context, Func<HttpContext, Task> next);
}
