namespace GoCloudNative.Bff.Authentication.IdentityProviders;

public class TokenResponse
{
    public TokenResponse(string? access_token, string? id_token, string? refresh_token, DateTime expiryDate)
    {
        this.access_token = access_token;
        this.id_token = id_token;
        this.refresh_token = refresh_token;
        ExpiryDate = expiryDate;
    }
    
    public string? access_token { get; }
    public string? id_token { get; }
    public string? refresh_token { get; }
    public DateTime ExpiryDate { get; }
}