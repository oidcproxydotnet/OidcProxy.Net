using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal static class MeEndpoint<TIdp>
{
    public static async Task<IResult> Get(HttpContext context,
        [FromServices] IClaimsTransformation claimsTransformation)
    {
        if (!context.Session.HasIdToken<TIdp>())
        {
            return Results.NotFound();
        }

        context.Response.Headers.CacheControl = $"no-cache, no-store, must-revalidate";

        var idToken = context.Session.GetIdToken<TIdp>();
        var payload = idToken.ParseJwtPayload();
        var claims = await claimsTransformation.Transform(payload);
        return Results.Ok(claims);
    }
}