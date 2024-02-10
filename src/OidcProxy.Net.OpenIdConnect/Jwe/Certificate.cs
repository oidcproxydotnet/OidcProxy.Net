using System.Security.Cryptography.X509Certificates;
using Jose;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public class Certificate : ITokenEncryptionKey
{
    private readonly X509Certificate2 _certificate;

    public Certificate(X509Certificate2 certificate)
    {
        _certificate = certificate;
    }
    
    public string Decrypt(string payload)
    {
        var decrypted = JWE.Decrypt(payload,
            _certificate.GetRSAPrivateKey(), 
            JweAlgorithm.RSA_OAEP, 
            JweEncryption.A256CBC_HS512);

        return decrypted.Plaintext;
    }
}