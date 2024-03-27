using Jose;
using Microsoft.IdentityModel.Tokens;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public sealed class EncryptionKey(SymmetricSecurityKey key) : IJweEncryptionKey
{
    public string Decrypt(string token)
    {
        var jweToken = JWE.Decrypt(token, key.Key);
        return jweToken.Plaintext;
    }
}