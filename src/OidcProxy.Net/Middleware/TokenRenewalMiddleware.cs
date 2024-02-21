using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Middleware;

internal class TokenRenewalMiddleware : ITokenRenewalMiddleware
{
    private readonly TokenFactory _tokenFactory;
    private readonly ILogger _logger;

    public TokenRenewalMiddleware(TokenFactory tokenFactory, ILogger logger)
    {
        _tokenFactory = tokenFactory;
        _logger = logger;
    }
    
    public async Task Apply(HttpContext context, Func<HttpContext, Task> next)
    {
        // Check expiry again because another thread may have updated the token
        try
        {
            await _tokenFactory.RenewAccessTokenIfExpiredAsync(context.TraceIdentifier);
        }
        catch (TokenRenewalFailedException e)
        {
            await _logger.ErrorAsync(e);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(@"{ ""reason"": ""token_renewal_failed"" }");
            return; // And stop the pipeline here. The request will not be forwarded down-stream.
        }

        await next(context);
    }
}