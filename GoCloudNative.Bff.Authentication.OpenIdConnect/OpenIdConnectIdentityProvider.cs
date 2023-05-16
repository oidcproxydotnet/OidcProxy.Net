using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Http;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Caching.Memory;
using TokenResponse = GoCloudNative.Bff.Authentication.IdentityProviders.TokenResponse;

namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

public class OpenIdConnectIdentityProvider : IIdentityProvider
{
    private readonly HttpClient _wellKnownHttpClient;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _tokenHttpClient;
    private readonly HttpClient _revocationHttpClient;

    private readonly OpenIdConnectConfig _configuration;
    
    public OpenIdConnectIdentityProvider(HttpClient wellKnownHttpClient, 
        IMemoryCache cache,
        HttpClient tokenHttpClient, 
        HttpClient revocationHttpClient, 
        OpenIdConnectConfig configuration)
    {
        _wellKnownHttpClient = wellKnownHttpClient;
        _cache = cache;
        _tokenHttpClient = tokenHttpClient;
        _revocationHttpClient = revocationHttpClient;
        _configuration = configuration;
    }
    
    public virtual async Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context, string redirectUri)
    { 
        var client = new OidcClient(new OidcClientOptions
        {
            Authority = _configuration.Authority,
            ClientId = _configuration.ClientId,
            ClientSecret = _configuration.ClientSecret,
            RedirectUri = redirectUri
        });

        var request = await client.PrepareLoginAsync();

        // Strange.. Oidc clients seems not to include the scope in the authorize request
        // As a result, the response will not contain an id_token, nor a refresh_token
        // So, we append the start-url with the scopes to get the tokens we need
        var requiredScopes = new[] { "openid", "offline_access" };
        var allScopes = _configuration.Scopes.Select(x => x.ToLowerInvariant()).Union(requiredScopes);
        var scopesParameter = $"scope={string.Join("%20", allScopes)}";
        
        var startUrl = $"{request.StartUrl}&{scopesParameter}";
        
        return new AuthorizeRequest(new Uri(startUrl), request.CodeVerifier);
    }

    public virtual async Task<TokenResponse> GetTokenAsync(HttpContext context, string redirectUri, string code, string? codeVerifier)
    {
        var wellKnown = await GetWellKnownConfiguration();

        if (wellKnown.token_endpoint == null)
        {
            throw new ApplicationException(
                "Unable to exchange code for access_token. The well-known/openid-configuration" +
                "document does not contain a token endpoint.");
        }

        _tokenHttpClient.SetBaseAddressIfNotSet(wellKnown.token_endpoint);
        
        var response = await _tokenHttpClient.RequestTokenAsync(new AuthorizationCodeTokenRequest
        {
             GrantType = OidcConstants.GrantTypes.AuthorizationCode,
             ClientId = _configuration.ClientId,
             ClientSecret = _configuration.ClientSecret,
             
             Parameters =
             {
                 { "code", code },
                 { "scope", string.Join(' ', _configuration.Scopes) },
                 { "redirect_uri", redirectUri },
                 { "code_verifier", codeVerifier },
             }
        });

        if (response.IsError)
        {
            throw new ApplicationException($"Unable to retrieve token. " +
                                           $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
        }

        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken);
    }

    public virtual async Task Revoke(string token)
    {
        var openIdConfiguration = await GetWellKnownConfiguration();

        _revocationHttpClient.SetBaseAddressIfNotSet(openIdConfiguration.revocation_endpoint);
        
        var response = await _revocationHttpClient.RevokeTokenAsync(new TokenRevocationRequest
        {
            Token = token,
            ClientId = _configuration.ClientId,
            ClientSecret = _configuration.ClientSecret
        });
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new ApplicationException($"Unable to revoke tokens. OIDC server responded {response.HttpStatusCode}:" +
                                           $" \r\n{response.Raw}");
        }
    }

    private async Task<OpenIdConfiguration> GetWellKnownConfiguration()
    {
        var endpointAddress = $"{_configuration.Authority.TrimEnd('/')}/" +
                              $"{_configuration.WellKnownEndpoint.TrimStart('/')}";

        if (_cache.TryGetValue(endpointAddress, out var wellKnownDocument))
        {
            return (OpenIdConfiguration)wellKnownDocument;
        }
        
        var httpResponse = await _wellKnownHttpClient.GetAsync(endpointAddress);         
        wellKnownDocument = await httpResponse.Content.ReadFromJsonAsync<OpenIdConfiguration>();
        
        if (wellKnownDocument == null)
        {
            throw new ApplicationException(
                "Unable to login. Unable to find a well-known/openid-configuration document " +
                $"at {endpointAddress}");
        }

        _cache.Set(endpointAddress, wellKnownDocument, TimeSpan.FromHours(1));

        return (OpenIdConfiguration)wellKnownDocument;
    }
}