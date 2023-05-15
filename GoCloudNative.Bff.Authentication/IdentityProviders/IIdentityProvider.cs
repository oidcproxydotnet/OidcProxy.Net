using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.IdentityProviders;

public interface IIdentityProvider
{
    public Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context);

    public Task<TokenResponse> GetTokenAsync(HttpContext context, string codeVerifier);
    
    public Task RevokeAccessToken(string accessToken);
    
    public Task RevokeRefreshToken(string refreshToken);
}