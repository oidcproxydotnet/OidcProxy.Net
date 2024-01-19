using System.IdentityModel.Tokens.Jwt;

namespace OidcProxy.Net.OpenIdConnect;

internal class DefaultClaimsTransformation : IClaimsTransformation
{ 
    public Task<object> Transform(JwtPayload payload) => Task.FromResult<object>(payload);
}