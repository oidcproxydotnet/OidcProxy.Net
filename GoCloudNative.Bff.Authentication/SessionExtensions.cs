using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public static class SessionExtensions
{
    private static readonly string VerifierKey = "verifier_key";

    private static readonly string TokenKey = "token_key";

    private static readonly string IdTokenKey = "id_token_key";
    
    private static readonly string RefreshTokenKey = "refresh_token_key";
    
    private static readonly string ExpiryKey = "expires";
    
    public static void Save(this ISession session, TokenResponse tokenResponse)
    {
        session.Save(TokenKey, tokenResponse.access_token);
        session.Save(IdTokenKey, tokenResponse.id_token);
        session.Save(RefreshTokenKey, tokenResponse.refresh_token);
        session.SetDateTime(ExpiryKey, tokenResponse.ExpiryDate);
    }
    
    public static void UpdateAccessAndRefreshToken(this ISession session, TokenResponse tokenResponse)
    {
        session.Save(TokenKey, tokenResponse.access_token);
        session.SetDateTime(ExpiryKey, tokenResponse.ExpiryDate);
        
        if (!string.IsNullOrEmpty(tokenResponse.refresh_token))
        {
            session.Save(RefreshTokenKey, tokenResponse.refresh_token);
        }
    }

    public static bool HasIdToken(this ISession session) => session.Keys.Contains(IdTokenKey);
    public static string? GetIdToken(this ISession session) => session?.GetString(IdTokenKey);
    public static void RemoveIdToken(this ISession session) => session.Remove(IdTokenKey);
    
    public static bool HasAccessToken(this ISession session) => session.Keys.Contains(TokenKey);
    public static string? GetAccessToken(this ISession session) => session?.GetString(TokenKey);
    public static void RemoveAccessToken(this ISession session) => session.Remove(TokenKey);

    public static bool HasRefreshToken(this ISession session) => session.Keys.Contains(RefreshTokenKey);
    public static string? GetRefreshToken(this ISession session) => session?.GetString(RefreshTokenKey);
    public static void RemoveRefreshToken(this ISession session) => session.Remove(RefreshTokenKey);

    public static bool HasExpiryDate(this ISession session) => session.Keys.Contains(ExpiryKey);
    public static DateTime? GetExpiryDate(this ISession session) => session.GetDateTime(ExpiryKey);
    
    public static bool HasCodeVerifier(this ISession session) => session.Keys.Contains(VerifierKey);
    public static string? GetCodeVerifier(this ISession session) => session?.GetString(VerifierKey);
    public static void SetCodeVerifier(this ISession session, string codeVerifier) 
        => session.SetString(VerifierKey, codeVerifier);
    public static void RemoveCodeVerifier(this ISession session) => session.Remove(VerifierKey);
    
    

    
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
        var stringValue = value?.ToString("s");
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