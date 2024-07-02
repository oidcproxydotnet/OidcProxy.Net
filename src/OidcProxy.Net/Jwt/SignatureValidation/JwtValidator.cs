using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.Jwt.SignatureValidation;

public class JwtValidator(IIdentityProvider identityProvider, Hs256SignatureValidator hs256SignatureValidator)
{
    public virtual async Task<bool> Validate(string token)
    {
        var jwtParser = new JwtParser();
        if (jwtParser.IsJwe(token))
        {
            // JWEs are decrypted by the <see cref="OidcProxy.Net.Middleware.OidcProxyAuthenticationHandler"/> class 
            // using the <see cref="OidcProxy.Net.Middleware.OidcProxyAuthenticationHandler"/>
            // If the JWE can't be decrypted by it, it means it has been tampered with.
            // This makes signature validation redundant. Hence the statement: If it's a JWT, it's valid!
            return true;
        }

        var signatureValidator = CreateValidator(token);
        if (signatureValidator == null)
        {
            return true;
        }

        var keys = await identityProvider.GetJwksAsync();
        return await signatureValidator.Validate(token, keys);
    }

    protected virtual SignatureValidator? CreateValidator(string token)
    {
        var header = JwtParser.ParseJwtHeader(token);
        if (header == null)
        {
            return new InvalidJwtSignatureValidator();
        }

        SignatureValidator signatureValidator;
        switch (header.Alg)
        {
            case "RS256":
                signatureValidator = new Rs256SignatureValidator();
                break;
            case "HS256":
                signatureValidator = hs256SignatureValidator;
                break;
            default:
                return null;
        }

        return signatureValidator;
    }
}