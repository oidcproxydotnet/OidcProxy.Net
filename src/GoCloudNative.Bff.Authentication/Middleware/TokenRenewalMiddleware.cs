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
    private readonly ILocker _locker;

    public TokenRenewalMiddleware(TIdentityProvider identityProvider, 
        ILogger<TokenRenewalMiddleware<TIdentityProvider>> logger,
        ILocker locker)
    {
        _identityProvider = identityProvider;
        _logger = logger;
        _locker = locker;
    }
    
    public async Task Apply(HttpContext context, Func<HttpContext, Task> next)
    {
        var factory = new TokenFactory(_identityProvider, context.Session);
        
        // Do nothing if the token still is valid
        if (!factory.GetIsTokenExpired<TIdentityProvider>())
        {
            await next(context);
            return;
        }

        // Assume a scenario where a SPA executes multiple API requests at the BFF at the same time.
        // We want to prevent the BFF refreshing the access token several times.
        // Therefore, we acquire a lock to make sure only one thread updates the access token, the other threads
        // can use it.
        //
        // This software can run in two modes:
        // Mode 1.) Single node. Only one instance of the BFF is running somewhere. In that case, the BFF is backed
        //    by In-Memory session storage, and a normal locking mechanism can be used
        // Mode 2.) Distributed mode. It is likely that this software will run in a distributed context. Multiple
        //    instances of the BFF will be active at the same time. Session storage will be backed by Redis, and locking
        //    is backed by Redis too.
        // Because of the different modes this software runs in, Locking is abstracted out.
        await _locker.AcquireLock(context.Session, 90 * 1000, async () =>
        {   
            // Check expiry again because another thread may have updated the token
            var stillIsExpired = factory.GetIsTokenExpired<TIdentityProvider>();
            if (stillIsExpired)
            {
                try
                {
                    await factory.RenewAccessTokenIfExpiredAsync<TIdentityProvider>();
                }
                catch (TokenRenewalFailedException e)
                {
                    _logger.LogException(context, e);

                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync(@"{ ""reason"": ""token_renewal_failed"" }");
                    return; // And stop the pipeline here. The request will not be forwarded down-stream.
                }
                
                _logger.LogLine(context, "Renewed access_token and refresh_token");
            }
        });

        await next(context);
    }
}