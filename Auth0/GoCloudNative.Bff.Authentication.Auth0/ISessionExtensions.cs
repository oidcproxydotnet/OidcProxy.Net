using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Auth0;

public static class ISessionExtensions
{
    public static string GetAccessToken(this ISession session) => session.GetAccessToken<Auth0IdentityProvider>();
}