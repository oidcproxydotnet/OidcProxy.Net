using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Endpoints;

internal static class Endpoints
{
    public static void MapAuthenticationEndpoints(this WebApplication app, string endpointName)
    {
        app.MapGet($"/{endpointName}/me", MeEndpoint.Get);
        
        app.MapGet($"/{endpointName}/login", LoginEndpoint.Get);

        app.MapGet($"/{endpointName}/login/callback", CallbackEndpoint.Get);

        app.MapGet($"/{endpointName}/login/callback/error", () => Results.Text("Login failed."));

        app.MapGet($"/{endpointName}/end-session", EndSessionEndpoint.Get);
    }
}