using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Locking;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.OpenIdConnect;

internal class TokenFactory
{
    private readonly ILogger _logger;
    private readonly IIdentityProvider _identityProvider;
    private readonly ISession _session;
    private readonly IConcurrentContext _concurrentContext;

    public TokenFactory(ILogger logger,
        IIdentityProvider identityProvider, 
        ISession session,
        IConcurrentContext concurrentContext)
    {
        _logger = logger;
        _identityProvider = identityProvider;
        _session = session;
        _concurrentContext = concurrentContext;
    }

    public async Task RenewAccessTokenIfExpiredAsync(string traceIdentifier)
    {
        await _concurrentContext.ExecuteOncePerSession(_session, 
            nameof(RenewAccessTokenIfExpiredAsync),
            GetIsTokenExpired, 
            async () =>
            {
                // avoid thread collisions without complicated distributed read/write locking mechanisms..
                // without this, the the token will be refreshed multiple times
                // i know.. a bit hacky.. but it gets the job done without adding too much complexity...
                await _session.ProlongExpirationDate(15);

                try
                {
                    var refreshToken = _session.GetRefreshToken(); // todo: What is refresh_token is null?
                    var tokenResponse = await _identityProvider.RefreshTokenAsync(refreshToken, traceIdentifier);
                
                    await _session.UpdateAccessAndRefreshTokenAsync(tokenResponse);

                    // in case of static refresh_tokens requesting a new access token will not always yield a refresh_token
                    if (!string.IsNullOrEmpty(tokenResponse.refresh_token) && refreshToken != tokenResponse.refresh_token) 
                    {
                        try
                        {
                            await _identityProvider.RevokeAsync(refreshToken, traceIdentifier);
                        }
                        catch (Exception e)
                        {
                            // Suppress and warn... Throwing the exception here is pointless because there's nothing 
                            // the user can do to revoke the token and the new refresh_token has been issued anyways.
                            
                            // Also, in many cases, vendors do not implement the revoke endpoint or they do not support 
                            // token revocation. (e.g. Auth0, Azure EntraId, OpenIdDict, and so forth...) This will
                            // also result in an error when revocation is attempted.
                            _logger.Warn(traceIdentifier, $"Failed to revoke refresh_token. {e}");
                        }
                    }
                }
                catch (Exception)
                {
                    await _session.ProlongExpirationDate(-15);
                    throw;
                }
            });
    }
    
    private bool GetIsTokenExpired()
    {
        var expiryDateInSession = _session.GetExpiryDate();
        if (!expiryDateInSession.HasValue)
        {
            return false;
        }
        
        var expiry = expiryDateInSession.Value.AddSeconds(-30);
        var now = DateTime.UtcNow;

        return expiry <= now;
    }
}