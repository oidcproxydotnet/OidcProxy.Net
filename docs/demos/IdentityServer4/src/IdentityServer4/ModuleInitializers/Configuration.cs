using IdentityServer4;
using IdentityServer4.Models;

namespace TestIdentityServer.ModuleInitializers;

public static class Configuration
{
    public static readonly IEnumerable<IdentityResource> IdentityResources = new []
    {
        (IdentityResource)new IdentityResources.OpenId(),
        new IdentityResources.Profile(),
        new IdentityResources.Email()
    };
    
    public static readonly IEnumerable<ApiScope> ApiScopes =
        new[]
        {
            // local API scope
            new ApiScope(IdentityServerConstants.LocalApi.ScopeName),
            new ApiScope("bff", "bff")
        };

    public static readonly Client Client = new Client
    {
        ClientId = "bff",

        ClientSecrets =
        {
            new Secret("secret".Sha256())
        },

        AllowedGrantTypes = GrantTypes.Code,
        RequirePkce = true,
        RequireConsent = true,

        AccessTokenLifetime = 75,
        AlwaysIncludeUserClaimsInIdToken = true,

        RedirectUris = { "https://localhost:8443/account/login/callback", "https://localhost:8443/oidc/login/callback" },
        FrontChannelLogoutUri = "https://localhost:8443/",
        PostLogoutRedirectUris = { "https://localhost:8443/" },

        AllowedScopes = 
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Email,
            "bff"
        }
    };
}