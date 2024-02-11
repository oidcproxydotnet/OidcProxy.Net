using Jose;
using System.Security.Cryptography.X509Certificates;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public sealed class Certificate : ITokenEncryptionKey
{
    private readonly X509Certificate2 _certificate;

    public Certificate(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public string Decrypt(string payload)
    {
        var jweToken = JWE.Decrypt(payload, _certificate.GetRSAPrivateKey());
        return jweToken.Plaintext;
    }
}