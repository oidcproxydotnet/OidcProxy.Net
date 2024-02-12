using Jose;
using Microsoft.IdentityModel.Tokens;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public sealed class EncryptionKey : IJweEncryptionKey
{
    private readonly SymmetricSecurityKey _key;

    public EncryptionKey(SymmetricSecurityKey key)
    {
        _key = key;
    }
    
    public string Decrypt(string token)
    {
        var jweToken = JWE.Decrypt(token, _key.Key);
        return jweToken.Plaintext;
    }
}