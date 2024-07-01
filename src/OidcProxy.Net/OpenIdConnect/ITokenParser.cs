using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.OpenIdConnect;

public interface ITokenParser
{
    string GetRoleClaim();
    string GetNameClaim();
    
    JwtPayload? ParseJwtPayload(string? accessToken);
    JwtPayload? ParseIdToken(string? idToken);
}