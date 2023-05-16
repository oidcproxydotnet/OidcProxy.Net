
using Microsoft.Extensions.Caching.Memory;
using TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.Auth0;

public class Auth0IdentityProvider : OpenIdConnectIdentityProvider
{
    private readonly Auth0Config _config;

    public Auth0IdentityProvider(HttpClient wellKnownHttpClient, 
        IMemoryCache cache,
        HttpClient client1, 
        HttpClient client3, 
        HttpClient client2, // todo: fix this, this is ugly...
        Auth0Config config) : base(wellKnownHttpClient, 
        cache, 
        client1, 
        client2, 
        client3, 
        MapConfiguration(config))
    {
        _config = config;
    }

    public override Task Revoke(string token)
    {
        // I.l.e.: Auth0 does not support token revocation
        
        return Task.CompletedTask;
    }

    private static OpenIdConnectConfig MapConfiguration(Auth0Config config)
    {
        return new OpenIdConnectConfig
        {
            Authority = config.Authority,
            ClientId = config.ClientId,
            ClientSecret = config.ClientSecret,
            Scopes = config.Scopes
        };
    }
}