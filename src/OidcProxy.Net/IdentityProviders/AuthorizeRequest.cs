using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.IdentityProviders;

/// <summary>
/// To redirect a user to the /authorize endpoint, the complete authorize URL needs to be specified. In case of PKCE, it should contain a code verifier too.
/// </summary>
public class AuthorizeRequest
{
    /// <summary>
    /// /authorize URI of the Identity Server. /authorize URI as defined in section 3.1.2.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
    /// </summary>
    public Uri AuthorizeUri { get; }
    
    /// <summary>
    /// If applicable, the code verifier, as specified in section 4.1 of the PKCE spec: https://www.rfc-editor.org/rfc/rfc7636#section-4.1
    /// </summary>
    public string? CodeVerifier { get; }

    public AuthorizeRequest(Uri authorizeUri)
    {
        AuthorizeUri = authorizeUri;
    }
    
    public AuthorizeRequest(Uri authorizeUri, string codeVerifier)
    {
        AuthorizeUri = authorizeUri;
        CodeVerifier = codeVerifier;
    }

    /// <summary>
    /// Wraps the /authorize URI of the Identity Server in an IResult. /authorize URI as defined in section 3.1.2.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
    /// </summary>
    /// <returns>An IResult</returns>
    public IResult ToResult() => Results.Redirect(AuthorizeUri.ToString());

    /// <summary>
    /// The /authorize URI of the Identity Server. /authorize URI as defined in section 3.1.2.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
    /// </summary>
    /// <returns>The authorize uri</returns>
    public Uri ToUri() => AuthorizeUri;

    /// <summary>
    /// The /authorize URI of the Identity Server. /authorize URI as defined in section 3.1.2.1. of the OpenId Connect spec: https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest
    /// </summary>
    /// <returns>The authorize uri</returns>
    public override string ToString() => AuthorizeUri.ToString();
}