using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.OpenIdConnect;

public interface IClaimsTransformation
{
    Task<object> Transform(JwtPayload payload);
}