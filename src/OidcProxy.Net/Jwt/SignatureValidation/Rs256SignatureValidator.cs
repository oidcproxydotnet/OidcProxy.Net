using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.Jwt.SignatureValidation;

public class Rs256SignatureValidator : SignatureValidator
{
    protected override KeySet? GetKeySet(IEnumerable<KeySet> keySets, JwtHeader header)
    {
        return keySets.SingleOrDefault(x => x.Kid == header.Kid);
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
}