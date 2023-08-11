using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal static class Endpoints
{
    public static void MapAuthenticationEndpoints<TIdp>(this WebApplication app, string endpointName) where TIdp : IIdentityProvider
    {
        app.MapGet($"/{endpointName}/me", MeEndpoint<TIdp>.Get);
        
        app.MapGet($"/{endpointName}/login", LoginEndpoint<TIdp>.Get);

        app.MapGet($"/{endpointName}/login/callback", CallbackEndpoint<TIdp>.Get);

        app.MapGet($"/{endpointName}/login/callback/error", () => Results.Text("Login failed."));

        app.MapGet($"/{endpointName}/end-session", EndSessionEndpoint<TIdp>.Get);
    }
}