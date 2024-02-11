using Jose;
using Microsoft.IdentityModel.Tokens;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public sealed class EncryptionKey : ITokenEncryptionKey
{
    private readonly SymmetricSecurityKey _key;

    public EncryptionKey(SymmetricSecurityKey key)
    {
        _key = key;
    }
    
    public string Decrypt(string payload)
    {
        var jweToken = JWE.Decrypt(payload, _key.Key);
        return jweToken.Plaintext;
    }
}