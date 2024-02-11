using Jose;
using System.Security.Cryptography.X509Certificates;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public sealed class EncryptionCertificate : IJweEncryptionKey
{
    private readonly X509Certificate2 _certificate;

    public EncryptionCertificate(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }

    public string Decrypt(string token)
    {
        var jweToken = JWE.Decrypt(token, _certificate.GetRSAPrivateKey());
        return jweToken.Plaintext;
    }
}