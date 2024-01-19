using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.OpenIdConnect;

namespace Host;

public class MyClaimsTransformation : IClaimsTransformation
{
    public Task<object> Transform(JwtPayload payload)
    {
        var result = new
        {
            Sub = payload.Sub,
            Name = payload.Claims.Where(x => x.Type == "name").Select(x => x.Value).FirstOrDefault()
        };

        return Task.FromResult<object>(result);
    }
}