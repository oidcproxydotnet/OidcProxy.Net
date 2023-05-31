using System.Reflection.Metadata;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public static class SessionExtensions
{
    private static string GetVerifierKey<T>() => $"{typeof(T)}-verifier_key";

    private static string GetTokenKey<T>() => $"{typeof(T)}-token_key";

    private static string GetIdTokenKey<T>() => $"{typeof(T)}-id_token_key";
    
    private static string GetRefreshTokenKey<T>() => $"{typeof(T)}-refresh_token_key";
    
    private static string GetExpiryKey<T>() => $"{typeof(T)}-expires";
    
    public static async Task SaveAsync<T>(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey<T>(), tokenResponse.access_token);
        await session.SaveAsync(GetIdTokenKey<T>(), tokenResponse.id_token);
        await session.SaveAsync(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        await session.SetDateTimeAsync(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
    }
    
    public static async Task UpdateAccessAndRefreshTokenAsync<T>(this ISession session, TokenResponse tokenResponse)
    {
        await session.SaveAsync(GetTokenKey<T>(), tokenResponse.access_token);
        await session.SetDateTimeAsync(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            await session.SaveAsync(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        }
    }

    public static bool HasIdToken<T>(this ISession session) => session.Keys.Contains(GetIdTokenKey<T>());
    public static string? GetIdToken<T>(this ISession session) => session?.GetString(GetIdTokenKey<T>());
    public static async Task RemoveIdTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetIdTokenKey<T>());
    
    public static bool HasAccessToken<T>(this ISession session) => session.Keys.Contains(GetTokenKey<T>());
    public static string? GetAccessToken<T>(this ISession session) => session?.GetString(GetTokenKey<T>());
    public static async Task RemoveAccessTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetTokenKey<T>());

    public static bool HasRefreshToken<T>(this ISession session) => session.Keys.Contains(GetRefreshTokenKey<T>());
    public static string? GetRefreshToken<T>(this ISession session) => session?.GetString(GetRefreshTokenKey<T>());
    public static async Task RemoveRefreshTokenAsync<T>(this ISession session) => await session.RemoveAsync(GetRefreshTokenKey<T>());

    public static bool HasExpiryDate<T>(this ISession session) => session.Keys.Contains(GetExpiryKey<T>());
    public static DateTime? GetExpiryDate<T>(this ISession session) => session.GetDateTime(GetExpiryKey<T>());
    
    public static bool HasCodeVerifier<T>(this ISession session) => session.Keys.Contains(GetVerifierKey<T>());
    public static string? GetCodeVerifier<T>(this ISession session) => session?.GetString(GetVerifierKey<T>());
    public static async Task SetCodeVerifierAsync<T>(this ISession session, string codeVerifier) 
        => await session.SaveAsync(GetVerifierKey<T>(), codeVerifier);
    public static async Task RemoveCodeVerifierAsync<T>(this ISession session) => await session.RemoveAsync(GetVerifierKey<T>());
    
    private static DateTime? GetDateTime(this ISession session, string key)
    {
        var date = session.GetString(key);
        if (date == null)
        {
            return null;
        }

        return DateTime.Parse(date);
    }
    
    private static async Task SetDateTimeAsync(this ISession session, string key, DateTime? value)
    {
        var stringValue = value?.ToString("yyyy-MM-dd HH:mm:sszzz");
        await session.SaveAsync(key, stringValue);
    }

    public static async Task RemoveAsync(this ISession session, string key)
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