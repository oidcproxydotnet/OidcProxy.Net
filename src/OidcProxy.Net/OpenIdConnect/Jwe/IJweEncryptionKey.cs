namespace OidcProxy.Net.OpenIdConnect.Jwe;

public interface IJweEncryptionKey
{
    public string Decrypt(string token);
}