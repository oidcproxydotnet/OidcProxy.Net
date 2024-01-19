using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net;

public static class ISessionExtensions
{
    private static string DateFormat => "yyyy-MM-dd HH:mm:ss.fff";
    
    private static string GetVerifierKey() => $"{typeof(IIdentityProvider)}-verifier_key";

    private static string GetTokenKey() => $"{typeof(IIdentityProvider)}-token_key";

    private static string GetIdTokenKey() => $"{typeof(IIdentityProvider)}-id_token_key";
    
    private static string GetRefreshTokenKey() => $"{typeof(IIdentityProvider)}-refresh_token_key";
    
    private static string GetExpiryKey() => $"{typeof(IIdentityProvider)}-expires";
    
    internal static async Task SaveAsync(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey(), tokenResponse.access_token);
        await session.SaveAsync(GetIdTokenKey(), tokenResponse.id_token);
        await session.SaveAsync(GetRefreshTokenKey(), tokenResponse.refresh_token);
        await session.SetDateTimeAsync(GetExpiryKey(), tokenResponse.ExpiryDate);
    }
    
    internal static async Task UpdateAccessAndRefreshTokenAsync(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey(), tokenResponse.access_token);
        await session.SetDateTimeAsync(GetExpiryKey(), tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            await session.SaveAsync(GetRefreshTokenKey(), tokenResponse.refresh_token);
        }
    }

    internal static bool HasIdToken(this ISession session) => session.Keys.Contains(GetIdTokenKey());
    internal static string? GetIdToken(this ISession session) => session?.GetString(GetIdTokenKey());
    
    internal static bool HasAccessToken(this ISession session) => session.Keys.Contains(GetTokenKey());
    public static string? GetAccessToken(this ISession session) => session?.GetString(GetTokenKey());

    internal static bool HasRefreshToken(this ISession session) => session.Keys.Contains(GetRefreshTokenKey());
    internal static string? GetRefreshToken(this ISession session) => session?.GetString(GetRefreshTokenKey());

    internal static DateTime? GetExpiryDate(this ISession session) => session.GetDateTime(GetExpiryKey());
    
    internal static async Task ProlongExpiryDate(this ISession session, int seconds)
    {
        var current = session.GetDateTime(GetExpiryKey());
        if (current == null)
        {
            throw new NotSupportedException("Cannot prolong access token validity. Can only prolong based on expiry date. " +
                                            "But the Expiry was not set in the session.");
        }

        await session.SetDateTimeAsync(GetExpiryKey(), current.Value.AddSeconds(seconds));
    }

    internal static string? GetCodeVerifier(this ISession session) => session?.GetString(GetVerifierKey());
    internal static async Task SetCodeVerifierAsync(this ISession session, string codeVerifier) 
        => await session.SaveAsync(GetVerifierKey(), codeVerifier);
    internal static async Task RemoveCodeVerifierAsync(this ISession session) => await session.RemoveAsync(GetVerifierKey());
    
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