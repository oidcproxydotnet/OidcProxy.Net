namespace OidcProxy.Net.OpenIdConnect.Jwe;

public interface ITokenEncryptionKey
{
    public string Decrypt(string payload);
}