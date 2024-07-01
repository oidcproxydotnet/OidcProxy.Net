using System.Security.Cryptography;
using Jose;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Jwt.SignatureValidation;

public class TokenValidator(IIdentityProvider identityProvider, ProxyOptions options)
{
    public async Task<bool> Validate(string token)
    {
        var jwtParser = new JwtParser(options);
        if (jwtParser.IsJwe(token))
        {
            // Todo: Implement JWE validation
            return true;
        }

        var header = jwtParser.ParseJwtHeader(token);
        if (header == null)
        {
            return false;
        }

        var keys = await identityProvider.GetJwksAsync();
        var signingKey = keys.SingleOrDefault(x => x.Kid == header.Kid);

        var rsaParameters = new RSAParameters
        {
            Exponent = signingKey.Exponent,
            Modulus = signingKey.Modulus
        };

        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);

        try
        {
            JWT.Decode(token, rsa);
            return true;
        }
        catch (Exception e)
        {
            return false;
        }
    }
}