using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.Jwt.SignatureValidation;

public class InvalidJwtSignatureValidator : SignatureValidator
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