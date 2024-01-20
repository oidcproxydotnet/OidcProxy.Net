using System.Net.Http.Json;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;

namespace OidcProxy.Net.EntraId;

public class EntraIdIdentityProvider : OpenIdConnectIdentityProvider
{
    private readonly HttpClient _httpClient;
    private readonly EntraIdConfig _configuration;

    protected override string DiscoveryEndpointAddress => _configuration.DiscoveryEndpoint;
    

    public EntraIdIdentityProvider(ILogger<OpenIdConnectIdentityProvider> logger,
        IMemoryCache cache, 
        HttpClient httpClient, 
        EntraIdConfig configuration) 
        : base(logger, cache, httpClient, configuration)
    {
        _httpClient = httpClient;
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
            .WithTenantId(_configuration.TenantId)
            .ExecuteAsync();
        
        return new AuthorizeRequest(startUrl, verifier);
    }

    public override Task RevokeAsync(string token, string traceIdentifier)
    {
        return Task.CompletedTask; // not supported by Azure
    }

    protected override async Task<DiscoveryDocument?> ObtainDiscoveryDocument(string endpointAddress)
    {
        var httpResponse = await _httpClient.GetAsync(endpointAddress);
        return await httpResponse.Content.ReadFromJsonAsync<DiscoveryDocument>();
    }
}