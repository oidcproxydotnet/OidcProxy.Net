using System.IdentityModel.Tokens.Jwt;
using Jose;
using OidcProxy.Net.Cryptography;

namespace OidcProxy.Net.Jwt.SignatureValidation;

internal abstract class SignatureValidator
{
    public Task<bool> Validate(string token, IEnumerable<KeySet> keys)
    {
        var header = GetHeader(token);
        if (header == null)
        {
            return Task.FromResult(false);
        }
    
        var keySet = GetKeySet(keys, header);
        if (keySet == null)
        {
            throw new KeySetNotFoundException("Failed to validate signature. " +
                                              "Unable to find appropriate key to validate the signature with.");
        }

        // The signature is validated using JoseJWT. It uses an object to do so.
        // This could be anything ranging from a byte array to an RSA object.
        var rsa = CreateSigningObject(keySet); 

        try
        {
            Decode(token, rsa);
            return Task.FromResult(true);
        }
        catch (Exception)
        {
            return Task.FromResult(false);
        }   
    }

    protected virtual void Decode(string token, object key)
    {
        JWT.Decode(token, key);
    }

    protected virtual JwtHeader? GetHeader(string token) => JwtParser.ParseJwtHeader(token);

    protected abstract KeySet? GetKeySet(IEnumerable<KeySet> keys, JwtHeader header);

    protected abstract object CreateSigningObject(KeySet signingKey);
}