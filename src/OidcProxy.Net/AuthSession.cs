using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net;

internal class AuthSession
{
    private static string DateFormat => "yyyy-MM-dd HH:mm:ss.fff";
    
    private static string VerifierKey => $"{typeof(IIdentityProvider)}-verifier_key";

    public static string TokenKey => $"{typeof(IIdentityProvider)}-token_key";

    private static string IdTokenKey => $"{typeof(IIdentityProvider)}-id_token_key";
    
    private static string RefreshTokenKey => $"{typeof(IIdentityProvider)}-refresh_token_key";
    
    private static string ExpiryKey => $"{typeof(IIdentityProvider)}-expires";
    
    private static string UserPreferredLandingPageKey => $"{typeof(IIdentityProvider)}-user_preferred_landing_page";
    
    public ISession Session { get; private set; }

    public static AuthSession Create(ISession session) => new(session);

    /// <summary>
    /// Creates a new instance of the SessionWrapper class.
    /// </summary>
    /// <param name="httpContextAccessor">An instance of the IHttpContextAccessor to access the session.
    /// To instantiate this class with the ISession directly, use the HttpSession.Create factory method.</param>
    /// <exception cref="NotSupportedException"></exception>
    public AuthSession(IHttpContextAccessor httpContextAccessor) : this(httpContextAccessor.HttpContext?.Session!)
    {
        if (httpContextAccessor.HttpContext?.Session == null)
        {
            throw new NotSupportedException("There is no session.");
        }
    }

    /// <summary>
    /// Creates a new instance of the HttpSession class.
    /// </summary>
    /// <param name="session">The session</param>
    private AuthSession(ISession session)
    {
        this.Session = session;
    }

    /// <summary>
    /// Checks http session for presence of an `id_token`.
    /// </summary>
    /// <returns>A boolean value indicating whether the http session contains an `id_token`.</returns>
    public bool HasIdToken() => Session.Keys.Contains(IdTokenKey);
    
    /// <summary>
    /// Checks the http-session for presence of an `access_token`.
    /// </summary>
    /// <returns>A boolean value indicating whether the http session contains an `access_token`.</returns>
    public bool HasAccessToken() => Session.Keys.Contains(TokenKey);
    
    /// <summary>
    /// Checks the http-session for presence of an `refresh_token`.
    /// </summary>
    /// <returns>A boolean value indicating whether the http session contains an `refresh_token`.</returns>    
    public bool HasRefreshToken() => Session.Keys.Contains(RefreshTokenKey);
    
    /// <summary>
    /// Collects the `id_token` from the http session
    /// </summary>
    /// <returns>Null, or the `id_token`</returns>
    public string? GetIdToken() => Session?.GetString(IdTokenKey);
    
    /// <summary>
    /// Collects the `access_token` from the http session
    /// </summary>
    /// <returns>Null, or the `access_token`</returns>
    public string? GetAccessToken() => Session?.GetString(TokenKey);
    
    /// <summary>
    /// Collects the `refresh_token` from the http session
    /// </summary>
    /// <returns>Null, or the `refresh_token`</returns>
    public string? GetRefreshToken() => Session?.GetString(RefreshTokenKey);
    
    /// <summary>
    /// Collects a datetime value from the http session that indicates when the `access_token` expires.
    /// </summary>
    /// <returns>A datetime value from the http session that indicates when the `access_token` expires.</returns>
    public DateTime? GetExpirationDate() => this.GetDateTime(ExpiryKey);
    
    /// <summary>
    /// Increases the value that indicates when the `access_token` expires.
    /// </summary>
    /// <param name="seconds">The number of seconds to prolong the expiration date.</param>
    /// <exception cref="NotSupportedException">Throws a `NotSupportedException` if the date is not present in the http-session</exception>
    internal async Task ProlongExpirationDate(int seconds)
    {
        var current = this.GetDateTime(ExpiryKey);
        if (current == null)
        {
            throw new NotSupportedException("Cannot prolong access token validity. Can only prolong based on expiry date. " +
                                            "But the Expiry was not set in the session.");
        }

        await this.SetDateTimeAsync(ExpiryKey, current.Value.AddSeconds(seconds));
    }
    
    /// <summary>
    /// Collects a value that indicates where the user wants to be redirected after authenticating successfully.
    /// </summary>
    /// <returns>The user-defined landing page.</returns>
    public string? GetUserPreferredLandingPage() => Session?.GetString(UserPreferredLandingPageKey);
    
    /// <summary>
    /// Removes the user-defined landing page from the http session.
    /// </summary>
    public async Task RemoveUserPreferredLandingPageAsyncAsync() => await this.RemoveAsync(UserPreferredLandingPageKey);
    
    /// <summary>
    /// Collects the `code_verifier` that was set when initiating the authorize-request from the http-session.
    /// </summary>
    /// <returns>The `code_verifier` that was set when initiating the authorize-request.</returns>
    public string? GetCodeVerifier() => Session?.GetString(VerifierKey);
    
    /// <summary>
    /// Saves the `code_verifier` to the http-session.
    /// </summary>
    /// <param name="codeVerifier"></param>
    public async Task SetCodeVerifierAsync(string codeVerifier) => await this.SaveAsync(VerifierKey, codeVerifier);
    
    /// <summary>
    /// Removes the `code_verifier` from the http_session.
    /// </summary>
    public async Task RemoveCodeVerifierAsync() => await this.RemoveAsync(VerifierKey);

    /// <summary>
    /// Save the token response to the Http Session.
    /// </summary>
    /// <param name="tokenResponse">The response received from the /token endpoint.</param>
    public async Task SaveAsync(TokenResponse tokenResponse)
    {
        await this.SaveAsync(TokenKey, tokenResponse.access_token);
        await this.SaveAsync(IdTokenKey, tokenResponse.id_token);
        await this.SaveAsync(RefreshTokenKey, tokenResponse.refresh_token);
        await this.SetDateTimeAsync(ExpiryKey, tokenResponse.ExpiryDate);
    }

    /// <summary>
    /// Updates the `access_token`, the `refresh_token`, and the expiration date in the Http Session.
    /// </summary>
    /// <param name="tokenResponse">The response received from the /token endpoint.</param>
    public async Task UpdateAccessAndRefreshTokenAsync(TokenResponse tokenResponse)
    {
        await this.SaveAsync(TokenKey, tokenResponse.access_token);
        await this.SetDateTimeAsync(ExpiryKey, tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            await this.SaveAsync(RefreshTokenKey, tokenResponse.refresh_token);
        }
    }
    
    /// <summary>
    /// Stores the user defined landing page in the the Session.
    /// </summary>
    /// <param name="userPreferredLandingPage">The user preferred landing-page</param>
    /// <exception cref="NotSupportedException">Throws a NotSupportedException if the user-preferred landing page is not defined</exception>
    public async Task SetUserPreferredLandingPageAsync(string? userPreferredLandingPage)
    {   
        if (string.IsNullOrEmpty(userPreferredLandingPage))
        {
            await this.RemoveAsync(UserPreferredLandingPageKey);
            return;
        }

        if (!LandingPage.TryParse(userPreferredLandingPage, out _))
        {
            throw new NotSupportedException($"Will not redirect user to {userPreferredLandingPage}");
        }

        await this.SaveAsync(UserPreferredLandingPageKey, userPreferredLandingPage);
    }

    private DateTime? GetDateTime(string key)
    {
        var date = Session.GetString(key);
        if (date == null)
        {
            return null;
        }

        return DateTime.ParseExact(date, DateFormat, null);
    }

    private async Task SetDateTimeAsync(string key, DateTime? value)
    {
        var stringValue = value?.ToString(DateFormat);
        await this.SaveAsync(key, stringValue);
    }

    private async Task RemoveAsync(string key)
    {
        Session.Remove(key);
        await Session.CommitAsync();
    }
    
    private async Task SaveAsync(string key, string? value)
    {
        if (value == null && Session.Keys.Contains(key))
        {
            Session.Remove(key);
        }
        
        else if(value != null)
        {
            Session.SetString(key, value);
        }
        
        else
        {
            Session.Remove(key);
        }
        
        await Session.CommitAsync();
    }
}