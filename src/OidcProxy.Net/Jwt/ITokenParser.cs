using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.Jwt;

public interface ITokenParser
{
    JwtPayload? ParseJwtPayload(string? accessToken);
    JwtPayload? ParseIdToken(string? idToken);
}