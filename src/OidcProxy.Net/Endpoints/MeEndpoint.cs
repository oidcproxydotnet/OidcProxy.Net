using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class MeEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] IJwtParser jwtParser,
        [FromServices] IClaimsTransformation claimsTransformation)
    {
        if (!context.Session.HasIdToken())
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = $"no-cache, no-store, must-revalidate";

        var idToken = context.Session.GetIdToken();
        var payload = jwtParser.ParseIdToken(idToken);
        var claims = await claimsTransformation.Transform(payload);
        return Results.Ok(claims);
    }
}