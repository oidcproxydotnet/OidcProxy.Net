namespace OidcProxy.Net.Cryptography;

public interface IEncryptionKey
{
    public string Decrypt(string token);
}