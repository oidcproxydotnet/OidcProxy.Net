using System.Collections.Immutable;
using System.Security.Claims;
using Microsoft.AspNetCore;
using OpenIddict.Abstractions;
using OpenIdDictServer.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using OpenIddict.Server.AspNetCore;

using static OpenIddict.Abstractions.OpenIddictConstants;

namespace OpenIdDictServer.Controllers;

public class AuthorizationController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public AuthorizationController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    [HttpGet("~/connect/authorize")]
    public async Task<IActionResult> Authorize()
    {
        var user = await _userManager.FindByNameAsync("johndoe@oidcproxy.net");
        var claimsIdentity = await CreateClaimsIdentity(user);
        claimsIdentity.SetDestinations(GetDestinations);

        var principal = new ClaimsPrincipal(claimsIdentity);
            
        var identifier = principal.FindFirst(Claims.Subject)!.Value;

        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Import a few select claims from the identity stored in the local cookie.
        identity.AddClaim(new Claim(Claims.Subject, identifier));
        identity.AddClaim(new Claim(Claims.Name, identifier).SetDestinations(Destinations.AccessToken));
        identity.AddClaim(new Claim(Claims.PreferredUsername, identifier).SetDestinations(Destinations.AccessToken));

        return SignIn(new ClaimsPrincipal(identity), 
            properties: null, 
            OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
    }

    [HttpPost("~/connect/token"), IgnoreAntiforgeryToken, Produces("application/json")]
    public async Task<IActionResult> Exchange()
    {
        var request = HttpContext.GetOpenIddictServerRequest();
        var user = await _userManager.FindByNameAsync("johndoe@oidcproxy.net");
        
        if (request.IsAuthorizationCodeGrantType())
        {
            var identity = await CreateClaimsIdentity(user);
            identity.SetScopes(request.GetScopes());
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        if (request.IsRefreshTokenGrantType())
        {
            var identity = await CreateClaimsIdentity(user);
            identity.SetDestinations(GetDestinations);

            return SignIn(new ClaimsPrincipal(identity), OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
        }

        throw new NotImplementedException("The specified grant type is not implemented.");
    }

    private async Task<ClaimsIdentity> CreateClaimsIdentity(ApplicationUser user)
    {
        // Create the claims-based identity that will be used by OpenIddict to generate tokens.
        var identity = new ClaimsIdentity(
            authenticationType: TokenValidationParameters.DefaultAuthenticationType,
            nameType: Claims.Name,
            roleType: Claims.Role);

        // Add the claims that will be persisted in the tokens.
        identity.SetClaim(Claims.Subject, await _userManager.GetUserIdAsync(user))
            .SetClaim(Claims.Email, await _userManager.GetEmailAsync(user))
            .SetClaim(Claims.Name, await _userManager.GetUserNameAsync(user))
            .SetClaim(Claims.PreferredUsername, await _userManager.GetUserNameAsync(user))
            .SetClaims(Claims.Role, (await _userManager.GetRolesAsync(user)).ToImmutableArray());

        return identity;
    }

    private static IEnumerable<string> GetDestinations(Claim claim)
    {
        switch (claim.Type)
        {
            case Claims.Name or Claims.PreferredUsername:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Profile))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Email:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Email))
                    yield return Destinations.IdentityToken;

                yield break;

            case Claims.Role:
                yield return Destinations.AccessToken;

                if (claim.Subject.HasScope(Scopes.Roles))
                    yield return Destinations.IdentityToken;

                yield break;

            case "AspNet.Identity.SecurityStamp": yield break;

            default:
                yield return Destinations.AccessToken;
                yield break;
        }
    }
}