using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;

namespace GoCloudNative.Bff.Authentication;

public class TokenRenewer
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ISession _session;

    public TokenRenewer(IIdentityProvider identityProvider, ISession session)
    {
        _identityProvider = identityProvider;
        _session = session;
    }
    
    public async Task Renew(string refreshToken, int timeoutInSeconds = 90)
    {
        var cacheKey = $"refreshing_token_{_session.Id}";
        var valueInSession = _session.GetString(cacheKey);
        var isWorking = !string.IsNullOrEmpty(valueInSession);

        // The first request in a session which needs a renewed access token will bump into this block
        if (!isWorking)
        {
            _session.SetString(cacheKey, "true");

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
            
            _session.Remove(cacheKey);

            return;
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