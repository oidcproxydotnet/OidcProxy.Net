using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Locking;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Middleware;

internal class TokenRenewalMiddleware : ITokenRenewalMiddleware
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ILogger<TokenRenewalMiddleware> _logger;
    private readonly IConcurrentContext _concurrentContext;

    public TokenRenewalMiddleware(IIdentityProvider identityProvider, 
        ILogger<TokenRenewalMiddleware> logger,
        IConcurrentContext concurrentContext)
    {
        _identityProvider = identityProvider;
        _logger = logger;
        _concurrentContext = concurrentContext;
    }
    
    public async Task Apply(HttpContext context, Func<HttpContext, Task> next)
    {
        var factory = new TokenFactory(_identityProvider, context.Session, _concurrentContext);

        // Check expiry again because another thread may have updated the token
        try
        {
            await factory.RenewAccessTokenIfExpiredAsync(context.TraceIdentifier);
        }
        catch (TokenRenewalFailedException e)
        {
            _logger.LogException(context, e);

            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync(@"{ ""reason"": ""token_renewal_failed"" }");
            return; // And stop the pipeline here. The request will not be forwarded down-stream.
        }

        await next(context);
    }
}