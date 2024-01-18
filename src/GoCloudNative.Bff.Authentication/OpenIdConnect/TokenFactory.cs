using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

internal class TokenFactory
{
    private readonly IIdentityProvider _identityProvider;
    private readonly ISession _session;
    private readonly IConcurrentContext _concurrentContext;

    public TokenFactory(IIdentityProvider identityProvider, 
        ISession session,
        IConcurrentContext concurrentContext)
    {
        _identityProvider = identityProvider;
        _session = session;
        _concurrentContext = concurrentContext;
    }

    public bool GetIsTokenExpired<T>()
    {
        var expiryDateInSession = _session.GetExpiryDate<T>();
        if (!expiryDateInSession.HasValue)
        {
            return false;
        }
        
        var expiry = expiryDateInSession.Value.AddSeconds(-15);
        var now = DateTimeOffset.UtcNow;
        
        return expiry <= now;
    }

    public async Task RenewAccessTokenIfExpiredAsync<T>(string traceIdentifier)
    {
        await _concurrentContext.ExecuteOncePerSession(_session, 
            nameof(RenewAccessTokenIfExpiredAsync),
            GetIsTokenExpired<T>, 
            async () =>
            {   
                var refreshToken = _session.GetRefreshToken<T>(); // todo: What is refresh_token is null?
                var tokenResponse = await _identityProvider.RefreshTokenAsync(refreshToken, traceIdentifier);
    
                await _session.UpdateAccessAndRefreshTokenAsync<T>(tokenResponse);

                // in case of static refresh_tokens requesting a new access token will not always yield a refresh_token
                if (!string.IsNullOrEmpty(tokenResponse.refresh_token) && refreshToken != tokenResponse.refresh_token) 
                {
                    await _identityProvider.RevokeAsync(refreshToken, traceIdentifier);
                }
            });
    }
}