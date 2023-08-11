using GoCloudNative.Bff.Authentication;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public static class ISessionExtensions
{
    public static string GetAccessToken(this ISession session) => session.GetAccessToken<OpenIdConnectIdentityProvider>();
}