using Jose;
using Microsoft.IdentityModel.Tokens;

namespace OidcProxy.Net.Cryptography;

public class SymmetricKey(SymmetricSecurityKey key) : IEncryptionKey
{
    public string Decrypt(string token)
    {
        var jweToken = JWE.Decrypt(token, key.Key);
        return jweToken.Plaintext;
    }

    public byte[] ToByteArray() => key.Key;
}