using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking;
using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace GoCloudNative.Bff.Authentication.Middleware;

internal class TokenRenewalMiddleware<TIdentityProvider> : ITokenRenewalMiddleware
    where TIdentityProvider : IIdentityProvider
{
    private readonly TIdentityProvider _identityProvider;
    private readonly ILogger<TokenRenewalMiddleware<TIdentityProvider>> _logger;
    private readonly IConcurrentContext _concurrentContext;

    public TokenRenewalMiddleware(TIdentityProvider identityProvider, 
        ILogger<TokenRenewalMiddleware<TIdentityProvider>> logger,
        IConcurrentContext concurrentContext)
    {
        _identityProvider = identityProvider;
        _logger = logger;
        _concurrentContext = concurrentContext;
    }
    
    public async Task Apply(HttpContext context, Func<HttpContext, Task> next)
    {
        var factory = new TokenFactory(_logger, _identityProvider, context.Session, _concurrentContext);
        
        // Do nothing if the token still is valid
        if (!factory.GetIsTokenExpired<TIdentityProvider>())
        {
            await next(context);
            return;
        }
        
        // Check expiry again because another thread may have updated the token
        try
        {
            await factory.RenewAccessTokenIfExpiredAsync<TIdentityProvider>(() =>
            {   
                _logger.LogLine(context, "Renewed access_token and refresh_token");
            });
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