using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Middleware;

internal class TokenRenewalMiddleware<TIdentityProvider> : ITokenRenewalMiddleware
{
    public TokenRenewalMiddleware()
    {
        
    }
    
    public async Task Apply(HttpContext context, Func<HttpContext, Task> next)
    {
        if (false)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(@"{ ""reason"": ""token_renewal_failed"" }");
            return;
        }

        await next(context);
    }
}