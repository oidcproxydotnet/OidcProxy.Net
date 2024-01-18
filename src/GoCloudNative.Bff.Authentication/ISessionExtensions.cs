using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public static class ISessionExtensions
{
    private static string DateFormat => "yyyy-MM-dd HH:mm:ss.fff";
    
    private static string GetVerifierKey<T>() => $"{typeof(T)}-verifier_key";

    private static string GetTokenKey<T>() => $"{typeof(T)}-token_key";

    private static string GetIdTokenKey<T>() => $"{typeof(T)}-id_token_key";
    
    private static string GetRefreshTokenKey<T>() => $"{typeof(T)}-refresh_token_key";
    
    private static string GetExpiryKey<T>() => $"{typeof(T)}-expires";
    
    internal static async Task SaveAsync<T>(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey<T>(), tokenResponse.access_token);
        await session.SaveAsync(GetIdTokenKey<T>(), tokenResponse.id_token);
        await session.SaveAsync(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        await session.SetDateTimeAsync(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
    }
    
    internal static async Task UpdateAccessAndRefreshTokenAsync<T>(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey<T>(), tokenResponse.access_token);
        await session.SetDateTimeAsync(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            await session.SaveAsync(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        }
    }

    internal static bool HasIdToken<T>(this ISession session) => session.Keys.Contains(GetIdTokenKey<T>());
    internal static string? GetIdToken<T>(this ISession session) => session?.GetString(GetIdTokenKey<T>());
    internal static async Task RemoveIdTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetIdTokenKey<T>());
    
    internal static bool HasAccessToken<T>(this ISession session) => session.Keys.Contains(GetTokenKey<T>());
    public static string? GetAccessToken<T>(this ISession session) => session?.GetString(GetTokenKey<T>());
    internal static async Task RemoveAccessTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetTokenKey<T>());

    internal static bool HasRefreshToken<T>(this ISession session) => session.Keys.Contains(GetRefreshTokenKey<T>());
    internal static string? GetRefreshToken<T>(this ISession session) => session?.GetString(GetRefreshTokenKey<T>());
    internal static async Task RemoveRefreshTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetRefreshTokenKey<T>());

    internal static bool HasExpiryDate<T>(this ISession session) => session.Keys.Contains(GetExpiryKey<T>());
    internal static DateTime? GetExpiryDate<T>(this ISession session) => session.GetDateTime(GetExpiryKey<T>());
    
    internal static async Task ProlongExpiryDate<T>(this ISession session, int seconds)
    {
        var current = session.GetDateTime(GetExpiryKey<T>());
        await session.SetDateTimeAsync(GetExpiryKey<T>(), current.Value.AddSeconds(seconds));
    }

    internal static bool HasCodeVerifier<T>(this ISession session) => session.Keys.Contains(GetVerifierKey<T>());
    internal static string? GetCodeVerifier<T>(this ISession session) => session?.GetString(GetVerifierKey<T>());
    internal static async Task SetCodeVerifierAsync<T>(this ISession session, string codeVerifier) 
        => await session.SaveAsync(GetVerifierKey<T>(), codeVerifier);
    internal static async Task RemoveCodeVerifierAsync<T>(this ISession session) => await session.RemoveAsync(GetVerifierKey<T>());
    
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