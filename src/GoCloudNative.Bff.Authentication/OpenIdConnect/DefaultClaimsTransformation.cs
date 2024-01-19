using System.IdentityModel.Tokens.Jwt;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

internal class DefaultClaimsTransformation : IClaimsTransformation
{ 
    public Task<object> Transform(JwtPayload payload) => Task.FromResult<object>(payload);
}