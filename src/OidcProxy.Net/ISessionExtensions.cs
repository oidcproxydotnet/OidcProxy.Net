using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net;

public static class ISessionExtensions
{
    public static string? GetAccessToken(this ISession session) => session?.GetString(AuthSession.TokenKey);
}