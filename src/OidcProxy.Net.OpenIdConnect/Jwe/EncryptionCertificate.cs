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
        var privateKey = _certificate.GetRSAPrivateKey();
        if (privateKey == null)
        {
            throw new NotSupportedException("Failed to decrypt JWE. " +
                                            "The provided certificate does not have a private key. " +
                                            "The private key of the certificate is required to decrypt the JWE. " +
                                            "Provide a certificate with a private key.");
        }

        var jweToken = JWE.Decrypt(token, privateKey);
        return jweToken.Plaintext;
    }
}