using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace GoCloudNative.Bff.Authentication;

public class TokenRenewer
{
    private readonly IIdentityProvider _identityProvider;
    private readonly IDistributedCache _cache;
    private readonly ISession _session;

    public TokenRenewer(IIdentityProvider identityProvider, IDistributedCache cache, ISession session)
    {
        _identityProvider = identityProvider;
        _cache = cache;
        _session = session;
    }
    
    public async Task Renew(string refreshToken)
    {
        var cacheKey = $"refreshing_token_{_session.Id}";
        var isWorking = !string.IsNullOrEmpty(await _cache.GetStringAsync(cacheKey));

        // The first request in a session which needs a renewed access token will bump into this block
        if (!isWorking)
        {
            await _cache.SetStringAsync(cacheKey, "true");

            var tokenResponse = await _identityProvider.RefreshTokenAsync(refreshToken);
            
            // Store the tokens in the session
            _session.Save(LoginEndpoints.TokenKey, tokenResponse.access_token);
            _session.Save(LoginEndpoints.RefreshTokenKey, tokenResponse.refresh_token);

            // Revoke the old refresh token in case of rolling refresh tokens
            var newRefreshToken = tokenResponse.refresh_token;
            if (refreshToken != newRefreshToken)
            {
                await _identityProvider.Revoke(refreshToken);
            }
            
            await _cache.RemoveAsync(cacheKey);

            return;
        }

        // But when you have a page which invokes several endpoints at the same time, we want to prevent the bff from
        // ddossing the token endpoint. That's why only one request per second may renew a token. The others should wait.
        // This is the second path, if another request is already fetching a new token, then this thread will wait
        // until the other completes. It waits for a maximum of 90 seconds (average request timeout) to complete.
        for (var i = 0; i < 900 && isWorking; i++)
        {
            await Task.Delay(100);
        }

        throw new TimeoutException("Unable to renew the token. The request timed out.");
    }
}