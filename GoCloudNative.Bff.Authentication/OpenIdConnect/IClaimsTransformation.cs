using System.IdentityModel.Tokens.Jwt;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public interface IClaimsTransformation
{
    Task<object> Transform(JwtPayload payload);
}