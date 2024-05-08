using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

public class ClaimsTransformation : IClaimsTransformation
{
    public Task<object> Transform(JwtPayload payload)
    {
        var result = new 
        {
            Claims = "Transformed"
        };

        return Task.FromResult((object)result);
    }
}