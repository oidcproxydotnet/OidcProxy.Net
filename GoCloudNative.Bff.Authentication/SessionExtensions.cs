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
    
    public static void Save<T>(this ISession session, TokenResponse tokenResponse)
    {
        session.Save(GetTokenKey<T>(), tokenResponse.access_token);
        session.Save(GetIdTokenKey<T>(), tokenResponse.id_token);
        session.Save(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        session.SetDateTime(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
    }
    
    public static void UpdateAccessAndRefreshToken<T>(this ISession session, TokenResponse tokenResponse)
    {
        session.Save(GetTokenKey<T>(), tokenResponse.access_token);
        session.SetDateTime(GetExpiryKey<T>(), tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            session.Save(GetRefreshTokenKey<T>(), tokenResponse.refresh_token);
        }
    }

    public static bool HasIdToken<T>(this ISession session) => session.Keys.Contains(GetIdTokenKey<T>());
    public static string? GetIdToken<T>(this ISession session) => session?.GetString(GetIdTokenKey<T>());
    public static void RemoveIdToken<T>(this ISession session) => session.Remove(GetIdTokenKey<T>());
    
    public static bool HasAccessToken<T>(this ISession session) => session.Keys.Contains(GetTokenKey<T>());
    public static string? GetAccessToken<T>(this ISession session) => session?.GetString(GetTokenKey<T>());
    public static void RemoveAccessToken<T>(this ISession session) => session.Remove(GetTokenKey<T>());

    public static bool HasRefreshToken<T>(this ISession session) => session.Keys.Contains(GetRefreshTokenKey<T>());
    public static string? GetRefreshToken<T>(this ISession session) => session?.GetString(GetRefreshTokenKey<T>());
    public static void RemoveRefreshToken<T>(this ISession session) => session.Remove(GetRefreshTokenKey<T>());

    public static bool HasExpiryDate<T>(this ISession session) => session.Keys.Contains(GetExpiryKey<T>());
    public static DateTime? GetExpiryDate<T>(this ISession session) => session.GetDateTime(GetExpiryKey<T>());
    
    public static bool HasCodeVerifier<T>(this ISession session) => session.Keys.Contains(GetVerifierKey<T>());
    public static string? GetCodeVerifier<T>(this ISession session) => session?.GetString(GetVerifierKey<T>());
    public static void SetCodeVerifier<T>(this ISession session, string codeVerifier) 
        => session.SetString(GetVerifierKey<T>(), codeVerifier);
    public static void RemoveCodeVerifier<T>(this ISession session) => session.Remove(GetVerifierKey<T>());
    
    private static DateTime? GetDateTime(this ISession session, string key)
    {
        var date = session.GetString(key);
        if (date == null)
        {
            return null;
        }

        return DateTime.Parse(date);
    }
    
    private static void SetDateTime(this ISession session, string key, DateTime? value)
    {
        var stringValue = value?.ToString("yyyy-MM-dd HH:mm:sszzz");
        Save(session, key, stringValue);
    }
    
    private static void Save(this ISession session, string key, string? value)
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
    }
}