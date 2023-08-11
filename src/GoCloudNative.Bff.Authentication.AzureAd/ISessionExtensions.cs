using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public static class ISessionExtensions
{
    public static string GetAccessToken(this ISession session) => session.GetAccessToken<AzureAdIdentityProvider>();
}