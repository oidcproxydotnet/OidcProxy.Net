namespace OidcProxy.Net.OpenIdConnect;

public interface IJweEncryptionKey
{
    public string Decrypt(string token);
}