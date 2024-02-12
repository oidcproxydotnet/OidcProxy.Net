using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;

namespace Host.TestApps.OpenIdDict.IntegrationTests.Fixtures.OpenIddict;


public static class Endpoints
{
    public static WebApplication MapAuthorizeEndpoint(this WebApplication app)
    {
        app.MapGet("~/connect/authorize", () =>
        {
            var scheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
            var identity = CreateClaimsIdentity();
            var result = Results.SignIn(new ClaimsPrincipal(identity), null, scheme);

            return Task.FromResult(result);
        });

        return app;
    }

    public static WebApplication MapTokenEndpoint(this WebApplication app)
    {
        app.MapPost("~/connect/token", () =>
        {
            var scheme = OpenIddictServerAspNetCoreDefaults.AuthenticationScheme;
            var identity = CreateClaimsIdentity();
            var result = Results.SignIn(new ClaimsPrincipal(identity), null, scheme);

            return Task.FromResult(result);
        });

        return app;
    }

    private static ClaimsIdentity CreateClaimsIdentity()
    {
        var claims = new[]
        {
            new Claim(OpenIddictConstants.Claims.Subject, "johndoe"),
            new Claim(OpenIddictConstants.Claims.Name, "johndoe")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken),
            new Claim(OpenIddictConstants.Claims.PreferredUsername, "johndoe")
                .SetDestinations(OpenIddictConstants.Destinations.AccessToken)
        };

        var identity = new ClaimsIdentity(TokenValidationParameters.DefaultAuthenticationType,
            OpenIddictConstants.Claims.Name,
            OpenIddictConstants.Claims.Role);

        identity.AddClaims(claims);
        return identity;
    }
}