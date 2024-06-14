using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net;

public interface IAuthSession
{
    /// <summary>
    /// Collects the `id_token` from the http session
    /// </summary>
    /// <returns>Null, or the `id_token`</returns>
    string? GetIdToken();

    /// <summary>
    /// Collects the `access_token` from the http session
    /// </summary>
    /// <returns>Null, or the `access_token`</returns>
    string? GetAccessToken();

    /// <summary>
    /// Collects a value that indicates where the user wants to be redirected after authenticating successfully.
    /// </summary>
    /// <returns>The user-defined landing page.</returns>
    string? GetUserPreferredLandingPage();

    /// <summary>
    /// Generates the authorize url and saves the code verifier.
    /// </summary>
    /// <param name="userPreferredLandingPage">The page the user should be redirected to after signing in</param>
    /// <returns>The authorize url wrapped in an object.</returns>
    Task<AuthorizeRequest> InitiateAuthenticationSequence(string userPreferredLandingPage);
}