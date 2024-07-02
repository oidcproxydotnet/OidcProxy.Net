using System.IdentityModel.Tokens.Jwt;
using Jose;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.Jwt.SignatureValidation;

public class Hs256SignatureValidator(SymmetricKey symmetricKey) : SignatureValidator
{
    protected override KeySet? GetKeySet(IEnumerable<KeySet> keySets, JwtHeader header)
    {
        return keySets.SingleOrDefault();
    }

    protected override object CreateSigningObject(KeySet signingKey)
    {
        return symmetricKey.ToByteArray();
    }

    protected override void Decode(string token, object key)
    {
        JWT.Decode(token, key, JwsAlgorithm.HS256);
    }
}