using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Identity.Client;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public class AzureAdIdentityProvider : OpenIdConnectIdentityProvider
{
    private readonly AzureAdConfig _configuration;

    protected override string DiscoveryEndpointAddress => _configuration.DiscoveryEndpoint;
    

    public AzureAdIdentityProvider(IMemoryCache cache, HttpClient httpClient, AzureAdConfig configuration) 
        : base(cache, httpClient, configuration)
    {
        _configuration = configuration;
    }

    public override async Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
    {
        var app = ConfidentialClientApplicationBuilder.Create(_configuration.ClientId)
            .WithClientSecret(_configuration.ClientSecret)
            .Build();

        var startUrl = await app.GetAuthorizationRequestUrl(_configuration.Scopes)
            .WithPkce(out var verifier)
            .WithRedirectUri(redirectUri)
            .ExecuteAsync();
        
        return new AuthorizeRequest(startUrl, verifier);
    }

    public override Task Revoke(string token)
    {
        return Task.CompletedTask; // not supported by Azure
    }
}