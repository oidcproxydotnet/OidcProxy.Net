using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.Jwt.SignatureValidation;

internal class JwtSignatureValidator(IIdentityProvider identityProvider, 
    ILogger logger, 
    IEncryptionKey? jwtEncryptionKey,
    Rs256SignatureValidator rs256SignatureValidator,
    Hs256SignatureValidator hs256SignatureValidator) : IJwtSignatureValidator
{
    public virtual async Task<bool> Validate(string token)
    {
        if (JweParser.IsJwe(token))
        {
            // JWEs are decrypted by the <see cref="OidcProxy.Net.Middleware.OidcProxyAuthenticationHandler"/> class 
            // using the <see cref="OidcProxy.Net.Middleware.OidcProxyAuthenticationHandler"/>
            // If the JWE can't be decrypted by it, it means it has been tampered with.
            // This makes signature validation redundant. Hence the statement: If it's a JWT, it's valid!

            try
            {
                jwtEncryptionKey.Decrypt(token);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        var header = JwtParser.ParseJwtHeader(token);
        if (header == null)
        {
            await logger.WarnAsync("Unable to determine how to validate the access_token. " +
                                   "The JWT does not have a header. " +
                                   "The signature has not been verified and the JWT is considered to be invalid.");
            return false; // Removing the header from a tampered JWT should not mark the token valid.
        }

        var signatureValidator = CreateValidator(header);
        if (signatureValidator == null)
        {
            // Changing the algorithm in the header of tampered JWT should not mark the token valid.
            await logger.ErrorAsync("Unable to determine how to validate the access_token. " +
                                    "Unable to find a validator for the algorithm specified in the JWT header. " +
                                    "The signature has not been verified and the JWT is considered to be invalid.");
            return false;
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

    private SignatureValidator? CreateValidator(JwtHeader header)
    {
        return header.Alg switch
        {
            "RS256" => rs256SignatureValidator,
            "HS256" => hs256SignatureValidator,
            _ => null
        };
    }
}