using System.Web;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using IdentityModel.Client;

namespace OidcProxy.Net.Auth0;

public class Auth0IdentityProvider(
    ILogger logger,
    IMemoryCache cache,
    HttpClient client,
    Auth0Config config)
    : OpenIdConnectIdentityProvider(logger,
        cache,
        client,
        MapConfiguration(config))
{
    protected override Parameters? GetFrontChannelParameters()
    {
        var baseParams = base.GetFrontChannelParameters();
        if (baseParams == null) {
            baseParams = new Parameters();
        }

        baseParams.Add("audience", config.Audience);
        return baseParams;
    }

    public override Task RevokeAsync(string token, string traceIdentifier)
    {
        // I.l.e.: Auth0 does not support token revocation
        
        return Task.CompletedTask;
    }

    protected override Task<Uri> BuildEndSessionUri(string? idToken, string redirectUri)
    {
        var uri = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(config.UseOidcLogoutEndpoint)
            .WithDomain(config.Domain)
            .WithFederated(config.FederatedLogout)
            .WithRedirectUrl(redirectUri)
            .WithIdTokenHint(idToken)
            .Build();

        return Task.FromResult(uri);
    }

    private static OpenIdConnectConfig MapConfiguration(Auth0Config config)
    {
        return new OpenIdConnectConfig
        {
            Authority = $"https://{config.Domain}",
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            Scopes = config.Scopes
        };
    }
}