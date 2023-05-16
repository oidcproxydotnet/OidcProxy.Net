using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

internal static class SessionExtensions
{
    public static void Save(this ISession session, string key, string? value)
    {
        if (value == null && session.Keys.Contains(key))
        {
            session.Remove(key);
        }
        
        else
        {
            session.SetString(key, value);
        }
    }
}