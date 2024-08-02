using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Locking;

namespace OidcProxy.Net.OpenIdConnect;

internal class TokenFactory(
    AuthSession authSession,
    IJwtSignatureValidator jwtSignatureValidator,
    IIdentityProvider identityProvider,
    IConcurrentContext concurrentContext)
{
    public async Task RenewAccessTokenIfExpiredAsync(string traceIdentifier)
    {   
        await concurrentContext.ExecuteOncePerSession(authSession.Session, 
            nameof(RenewAccessTokenIfExpiredAsync),
            GetIsTokenExpired, 
            async () =>
            {
                // avoid thread collisions without complicated distributed read/write locking mechanisms..
                // without this, the the token will be refreshed multiple times
                // i know.. a bit hacky.. but it gets the job done without adding too much complexity...
                await authSession.ProlongExpirationDate(15);

                try
                {
                    var refreshToken = authSession.GetRefreshToken(); // todo: What is refresh_token is null?
                    var tokenResponse = await identityProvider.RefreshTokenAsync(refreshToken, traceIdentifier);

                    if (!(await jwtSignatureValidator.Validate(tokenResponse.access_token)))
                    {
                        throw new TokenRenewalFailedException("Failed to renew token. The new token has an invalid signature.");
                    }

                    await authSession.UpdateAccessAndRefreshTokenAsync(tokenResponse);
                }
                catch (Exception)
                {
                    await authSession.ProlongExpirationDate(-15);
                    throw;
                }
            });
    }

    private bool GetIsTokenExpired()
    {
        var expiryDateInSession = authSession.GetExpirationDate();
        if (!expiryDateInSession.HasValue)
        {
            return false;
        }
        
        var expiry = expiryDateInSession.Value.AddSeconds(-30);
        var now = DateTime.UtcNow;

        return expiry <= now;
    }
}