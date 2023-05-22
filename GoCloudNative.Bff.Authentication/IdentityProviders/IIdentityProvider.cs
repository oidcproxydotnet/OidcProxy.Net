using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.IdentityProviders;

public interface IIdentityProvider
{
    public Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri);

    public Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier);
    
    public Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    
    public Task Revoke(string token);
    public Task<Uri> GetEndSessionEndpoint(string? idToken, string baseAddress);
}