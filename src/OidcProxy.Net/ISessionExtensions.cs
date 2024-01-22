using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net;

public static class ISessionExtensions
{
    private static string DateFormat => "yyyy-MM-dd HH:mm:ss.fff";
    
    private static string VerifierKey => $"{typeof(IIdentityProvider)}-verifier_key";

    private static string TokenKey => $"{typeof(IIdentityProvider)}-token_key";

    private static string IdTokenKey => $"{typeof(IIdentityProvider)}-id_token_key";
    
    private static string RefreshTokenKey => $"{typeof(IIdentityProvider)}-refresh_token_key";
    
    private static string ExpiryKey => $"{typeof(IIdentityProvider)}-expires";
    
    private static string UserPreferredLandingPageKey => $"{typeof(IIdentityProvider)}-user_preferred_landing_page";
    
    internal static async Task SaveAsync(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(TokenKey, tokenResponse.access_token);
        await session.SaveAsync(IdTokenKey, tokenResponse.id_token);
        await session.SaveAsync(RefreshTokenKey, tokenResponse.refresh_token);
        await session.SetDateTimeAsync(ExpiryKey, tokenResponse.ExpiryDate);
    }
    
    internal static async Task UpdateAccessAndRefreshTokenAsync(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(TokenKey, tokenResponse.access_token);
        await session.SetDateTimeAsync(ExpiryKey, tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            await session.SaveAsync(RefreshTokenKey, tokenResponse.refresh_token);
        }
    }

    internal static bool HasIdToken(this ISession session) => session.Keys.Contains(IdTokenKey);
    internal static string? GetIdToken(this ISession session) => session?.GetString(IdTokenKey);
    
    internal static bool HasAccessToken(this ISession session) => session.Keys.Contains(TokenKey);
    public static string? GetAccessToken(this ISession session) => session?.GetString(TokenKey);

    internal static bool HasRefreshToken(this ISession session) => session.Keys.Contains(RefreshTokenKey);
    internal static string? GetRefreshToken(this ISession session) => session?.GetString(RefreshTokenKey);

    internal static DateTime? GetExpiryDate(this ISession session) => session.GetDateTime(ExpiryKey);
    
    internal static async Task ProlongExpiryDate(this ISession session, int seconds)
    {
        var current = session.GetDateTime(ExpiryKey);
        if (current == null)
        {
            throw new NotSupportedException("Cannot prolong access token validity. Can only prolong based on expiry date. " +
                                            "But the Expiry was not set in the session.");
        }

        await session.SetDateTimeAsync(ExpiryKey, current.Value.AddSeconds(seconds));
    }

    internal static string? GetCodeVerifier(this ISession session) => session?.GetString(VerifierKey);
    internal static async Task SetCodeVerifierAsync(this ISession session, string codeVerifier) 
        => await session.SaveAsync(VerifierKey, codeVerifier);
    internal static async Task RemoveCodeVerifierAsync(this ISession session) => await session.RemoveAsync(VerifierKey);
    
    
    internal static string? GetUserPreferredLandingPage(this ISession session) 
        => session?.GetString(UserPreferredLandingPageKey);
    internal static async Task SetUserPreferredLandingPageAsync(this ISession session, string? userPreferredLandingPage)
    {   
        if (string.IsNullOrEmpty(userPreferredLandingPage))
        {
            await session.RemoveAsync(UserPreferredLandingPageKey);
            return;
        }

        if (!LandingPage.TryParse(userPreferredLandingPage, out _))
        {
            throw new NotSupportedException($"Will not redirect user to {userPreferredLandingPage}");
        }

        await session.SaveAsync(UserPreferredLandingPageKey, userPreferredLandingPage);
    }

    internal static async Task RemoveUserPreferredLandingPageAsyncAsync(this ISession session) 
        => await session.RemoveAsync(UserPreferredLandingPageKey);
    
    
    private static DateTime? GetDateTime(this ISession session, string key)
    {
        var date = session.GetString(key);
        if (date == null)
        {
            return null;
        }

        return DateTime.ParseExact(date, DateFormat, null);
    }
    
    private static async Task SetDateTimeAsync(this ISession session, string key, DateTime? value)
    {
        var stringValue = value?.ToString(DateFormat);

        await session.SaveAsync(key, stringValue);
    }


    internal static async Task RemoveAsync(this ISession session, string key)
    {
        session.Remove(key);
        await session.CommitAsync();
    }

    private static async Task SaveAsync(this ISession session, string key, string? value)
    {
        if (value == null && session.Keys.Contains(key))
        {
            session.Remove(key);
        }
        
        else if(value != null)
        {
            session.SetString(key, value);
        }
        
        else
        {
            session.Remove(key);
        }
        
        await session.CommitAsync();
    }
}