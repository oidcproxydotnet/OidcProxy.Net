namespace OidcProxy.Net.IdentityProviders;

/// <summary>
/// Implement ((and register) this interface to implement a custom OIDC implementation.
/// </summary>
public interface IIdentityProvider
{
    /// <summary>
    /// Generates the /authorize URI of the Identity Server. /authorize URI must be as defined in section 3.1.2.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
    /// </summary>
    /// <param name="redirectUri">The URI the identity server will redirect to after the user has logged in successfully.</param>
    /// <returns>The /authorize URI with all parameters required for the Authorization Code flow.</returns>
    Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri);

    /// <summary>
    /// Exchanges the "code" (as defined in section 3.1.2.5. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthResponse) for an access token
    /// </summary>
    /// <param name="redirectUri">The URI the identity server must redirect to after the user has logged in successfully.</param>
    /// <param name="code">The querystring parameter "code" that was returned by the OIDC server in the redirect URI as defined in the OpenId Connect spec, section 3.1.2.5.: https://openid.net/specs/openid-connect-core-1_0.html#AuthResponse</param>
    /// <param name="codeVerifier">In case of PKCE, provide the code verifier, as specified in section 4.1 of the PKCE spec: https://www.rfc-editor.org/rfc/rfc7636#section-4.1</param>
    /// <returns>An id_token, an access_token, and an id_token as specified in section 3.1.3.3. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#TokenResponse.</returns>
    Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier, string traceIdentifier);
    
    /// <summary>
    /// Exchanges the refresh_token for a new access_token as defined in section 12.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#RefreshingAccessToken.
    /// </summary>
    /// <param name="refreshToken">The refresh_token.</param>
    /// <returns>An id_token, an access_token as specified in section 12.2 of the OpenID Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#RefreshTokenResponse.</returns>
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier);
    
    /// <summary>
    /// Revokes a token
    /// </summary>
    /// <param name="token">Either an access_token or a refresh_token</param>
    /// <returns>Void</returns>
    Task RevokeAsync(string token, string traceIdentifier);
    
    /// <summary>
    /// Returns the endpoint the end-user must be redirected to, to end the session at the OIDC server.
    /// </summary>
    /// <param name="idToken">The id_token that must be revoked</param>
    /// <param name="baseAddress">The base address of the OIDC server</param>
    /// <returns>The endpoint the end-user must be redirected to, to end the session at the OIDC server.</returns>
    Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress);
}