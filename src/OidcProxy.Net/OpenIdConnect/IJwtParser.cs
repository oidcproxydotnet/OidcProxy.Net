using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.OpenIdConnect;

public interface IJwtParser
{
    JwtPayload? ParseAccessToken(string? accessToken);
    JwtPayload? ParseIdToken(string? idToken);
}