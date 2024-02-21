using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class MeEndpoint
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] AuthSession authSession,
        [FromServices] ITokenParser tokenParser,
        [FromServices] IClaimsTransformation claimsTransformation)
    {
        if (!authSession.HasIdToken())
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = $"no-cache, no-store, must-revalidate";

        var idToken = authSession.GetIdToken();
        var payload = tokenParser.ParseIdToken(idToken);
        var claims = await claimsTransformation.Transform(payload);
        return Results.Ok(claims);
    }
}