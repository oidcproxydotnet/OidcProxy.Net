using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Middleware;

internal interface ITokenRenewalMiddleware
{
    Task Apply(HttpContext context, Func<HttpContext, Task> next);
}