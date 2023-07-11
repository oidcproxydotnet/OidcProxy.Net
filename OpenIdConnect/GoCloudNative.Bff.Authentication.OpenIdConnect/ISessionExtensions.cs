using Microsoft.AspNetCore.Http;
using TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public static class ISessionExtensions
{
    public static string GetAccessToken(this ISession session) => session.GetAccessToken<OpenIdConnectIdentityProvider>();
}