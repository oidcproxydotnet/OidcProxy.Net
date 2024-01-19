namespace OidcProxy.Net.IdentityProviders;

/// <summary>
/// This class contains the token endpoint response. In case of refreshing a token, there will be no id_token.
/// </summary>
public class TokenResponse
{
    public TokenResponse(string? access_token, string? id_token, string? refresh_token, DateTime expiryDate)
    {
        this.access_token = access_token;
        this.id_token = id_token;
        this.refresh_token = refresh_token;
        ExpiryDate = expiryDate;
    }
    
    /// <summary>
    /// The access_token, an opaque string that contains the users' authorizations
    /// </summary>
    public string? access_token { get; }
    
    /// <summary>
    /// The id_token. Usually contains the claims specified in section 5.1 of the OpenID Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#StandardClaims
    /// </summary>
    public string? id_token { get; }
    
    /// <summary>
    /// A token that can be used to renew the access_token. Specified in section 12 of the OpenID Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#RefreshTokens
    /// </summary>
    public string? refresh_token { get; }
    
    /// <summary>
    /// The time the access_token expires and cannot be used anymore.
    /// </summary>
    public DateTime ExpiryDate { get; }
}