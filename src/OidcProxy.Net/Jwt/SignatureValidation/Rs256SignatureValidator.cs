using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using Jose;
using OidcProxy.Net.Cryptography;

namespace OidcProxy.Net.Jwt.SignatureValidation;

internal class Rs256SignatureValidator : SignatureValidator
{
    protected override KeySet? GetKeySet(IEnumerable<KeySet> keySets, JwtHeader header)
    {
        var keys = keySets
            .Where(x => x.Kid == header.Kid)
            .ToArray();
        
        return (keys.Length == 1)
            ? keys.Single()
            : null;
    }

    protected override object CreateSigningObject(KeySet signingKey)
    {
        var rsaParameters = new RSAParameters
        {
            Exponent = signingKey.Exponent,
            Modulus = signingKey.Modulus
        };

        var rsa = RSA.Create();
        rsa.ImportParameters(rsaParameters);
        return rsa;
    }

    protected override void Decode(string token, object key)
    {
        JWT.Decode(token, key, JwsAlgorithm.RS256);
    }
}