using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public class TokenFactory
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ISession _session;

    public TokenFactory(IIdentityProvider identityProvider, ISession session)
    {
        _identityProvider = identityProvider;
        _session = session;
    }
    
    public async Task<bool> RenewAccessTokenIfExpiredAsync<T>(int timeoutInSeconds = 90)
    {
        var expiryDateInSession = _session.GetExpiryDate<T>();
        if (!expiryDateInSession.HasValue)
        {
            return false;
        }
        
        var expiry = expiryDateInSession.Value.AddSeconds(-15);
        var now = DateTimeOffset.UtcNow;
        
        if (expiry > now)
        {
            return false;
        }

        var refreshToken = _session.GetRefreshToken<T>(); // todo: What is refresh_token is null?
        await RenewAsync<T>(refreshToken, timeoutInSeconds);
        return true;
    }
    
    private async Task RenewAsync<T>(string refreshToken, int timeoutInSeconds = 90)
    {
        var cacheKey = $"refreshing_token_{_session.Id}";
        var valueInSession = _session.GetString(cacheKey);
        var isWorking = !string.IsNullOrEmpty(valueInSession);

        // The first request in a session which needs a renewed access token will bump into this block
        if (!isWorking)
        {
            _session.SetString(cacheKey, "true");

            try
            {
                var tokenResponse = await _identityProvider.RefreshTokenAsync(refreshToken);
            
                await _session.UpdateAccessAndRefreshTokenAsync<T>(tokenResponse);

                // in case of static refresh_tokens requesting a new access token will not always yield a refresh_token
                if (!string.IsNullOrEmpty(tokenResponse.refresh_token) && refreshToken != tokenResponse.refresh_token) 
                {
                    await _identityProvider.RevokeAsync(refreshToken);
                }

                return;
            }
            finally
            {
                await _session.RemoveAsync(cacheKey);
            }
        }

        // But when you have a page which invokes several endpoints at the same time, we want to prevent the bff from
        // ddossing the token endpoint. That's why only one request per second may renew a token. The others should wait.
        // This is the second path, if another request is already fetching a new token, then this thread will wait
        // until the other completes. It waits for a maximum of 90 seconds (average request timeout) to complete.
        var maxValue = timeoutInSeconds * 10; // increment with steps of 100 ms
        for (var i = 0; i < maxValue; i++)
        {
            await Task.Delay(100);
            
            valueInSession = _session.GetString(cacheKey);
            isWorking = !string.IsNullOrEmpty(valueInSession);

            if (!isWorking)
            {
                return;
            }
        }

        throw new TimeoutException("Unable to renew the token. The request timed out.");
    }
}