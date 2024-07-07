using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.Jwt.SignatureValidation;

internal class JwtSignatureValidator(IIdentityProvider identityProvider, 
    ILogger logger, 
    Rs256SignatureValidator rs256SignatureValidator,
    Hs256SignatureValidator hs256SignatureValidator) : IJwtSignatureValidator
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
            await logger.WarnAsync("Unable to determine how to validate the access_token. The access_token signature has not been verified.");
            return true;
        }

        var keys = await identityProvider.GetJwksAsync();
        try
        {
            return await signatureValidator.Validate(token, keys);
        }
        catch (KeySetNotFoundException)
        {
            // If the signature verification fails because the key wasn't found, it is very well possible the keys have
            // rotated in that case it is common practice to re-obtain the key-set and verify again.
            keys = await identityProvider.GetJwksAsync(true);
            return await signatureValidator.Validate(token, keys);
        }
    }

    private SignatureValidator? CreateValidator(string token)
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
                signatureValidator = rs256SignatureValidator;
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