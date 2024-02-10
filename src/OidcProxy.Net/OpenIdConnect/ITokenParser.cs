using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.OpenIdConnect;

public interface ITokenParser
{
    string GetRoleClaim();
    string GetNameClaim();
    
    [Obsolete("Method will be removed. Migrate to ParseAccessTokenAsync(..)")]
    JwtPayload? ParseAccessToken(string? accessToken);
    Task<JwtPayload?> ParseAccessTokenAsync(string? accessToken);
    JwtPayload? ParseIdToken(string? idToken);
}