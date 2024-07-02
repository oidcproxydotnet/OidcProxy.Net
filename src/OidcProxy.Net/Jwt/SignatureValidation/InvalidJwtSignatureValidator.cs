using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.Cryptography;

namespace OidcProxy.Net.Jwt.SignatureValidation;

internal class InvalidJwtSignatureValidator : SignatureValidator
{
    protected override KeySet? GetKeySet(IEnumerable<KeySet> keys, JwtHeader header)
    {
        return null;
    }

    protected override object CreateSigningObject(KeySet signingKey)
    {
        throw new NotSupportedException();
    }
}