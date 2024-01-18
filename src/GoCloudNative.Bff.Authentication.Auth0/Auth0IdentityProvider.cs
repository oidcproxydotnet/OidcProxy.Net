
using System.Web;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Auth0;

public class Auth0IdentityProvider : OpenIdConnectIdentityProvider
{
    private readonly Auth0Config _config;

    public Auth0IdentityProvider(ILogger<OpenIdConnectIdentityProvider> logger,
        IMemoryCache cache,
        HttpClient client, 
        Auth0Config config) : base(logger,
        cache, 
        client, 
        MapConfiguration(config))
    {
        _config = config;
    }

    public override async Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
    {
        var result = await base.GetAuthorizeUrlAsync(redirectUri);

        var audience = HttpUtility.UrlEncode(_config.Audience);
        var authorizeRequestWithAudience = $"{result.AuthorizeUri}&audience={audience}";

        return new AuthorizeRequest(new Uri(authorizeRequestWithAudience), result.CodeVerifier);
    }

    public override Task RevokeAsync(string token, string traceIdentifier)
    {
        // I.l.e.: Auth0 does not support token revocation
        
        return Task.CompletedTask;
    }

    protected override Task<Uri> BuildEndSessionUri(string? idToken, string redirectUri)
    {
        // Auth0 does not define their end_session_endpoint in the well-known/openid-configuration

        var federated = _config.FederatedLogout ? "?federated" : string.Empty;
        var endSessionUrl = $"https://{_config.Domain}/oidc/logout{federated}";
        var endSessionUri = new Uri(endSessionUrl);
        
        return Task.FromResult(endSessionUri);
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