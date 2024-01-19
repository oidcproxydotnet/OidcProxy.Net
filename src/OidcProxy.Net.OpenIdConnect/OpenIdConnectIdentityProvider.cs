using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Web;
using OidcProxy.Net.IdentityProviders;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using TokenResponse = OidcProxy.Net.IdentityProviders.TokenResponse;

namespace OidcProxy.Net.OpenIdConnect;

public class OpenIdConnectIdentityProvider : IIdentityProvider
{
    private readonly ILogger<OpenIdConnectIdentityProvider> _logger;
    private readonly IMemoryCache _cache;
    private readonly HttpClient _httpClient;

    private readonly OpenIdConnectConfig _configuration;
    
    protected virtual string DiscoveryEndpointAddress 
        => $"{_configuration.Authority.TrimEnd('/')}/" + $"{_configuration.DiscoveryEndpoint.TrimStart('/')}";
    
    public OpenIdConnectIdentityProvider(ILogger<OpenIdConnectIdentityProvider> logger,
        IMemoryCache cache,
        HttpClient httpClient, 
        OpenIdConnectConfig configuration)
    {
        _logger = logger;
        _cache = cache;
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public virtual async Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
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

    public virtual async Task<TokenResponse> GetTokenAsync(string redirectUri, 
        string code, 
        string? codeVerifier, 
        string traceIdentifier)
    {
        var wellKnown = await GetDiscoveryDocument();

        if (wellKnown.token_endpoint == null)
        {
            throw new ApplicationException(
                "Unable to exchange code for access_token. The well-known/openid-configuration" +
                "document does not contain a token endpoint.");
        }
        
        var response = await _httpClient.RequestTokenAsync(new AuthorizationCodeTokenRequest
        {
             Address = wellKnown.token_endpoint,
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

        _logger.LogInformation($"null [{DateTime.Now:s}] TraceId: {traceIdentifier} null "+
                               $"\"Queried /token endpoint and obtained id_, access_, and refresh_tokens\"");
        
        var expiryDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiryDate);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier)
    {
        var openIdConfiguration = await GetDiscoveryDocument();

        var response = await _httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = openIdConfiguration.token_endpoint,
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            RefreshToken = refreshToken,
            ClientId = _configuration.ClientId,
            ClientSecret = _configuration.ClientSecret,
        });
        
        if (response.IsError)
        {
            throw new TokenRenewalFailedException($"Unable to retrieve token. " +
                                                  $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
        }
        
        _logger.LogInformation($"null [{DateTime.Now:s}] TraceId: {traceIdentifier} null "+
                               $"\"Queried /token endpoint (refresh grant) and obtained id_, access_, and refresh_tokens\"");

        var expiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiresIn);
    }

    public virtual async Task RevokeAsync(string token, string traceIdentifier)
    {
        var openIdConfiguration = await GetDiscoveryDocument();

        var response = await _httpClient.RevokeTokenAsync(new TokenRevocationRequest
        {
            Address = openIdConfiguration.revocation_endpoint,
            Token = token,
            ClientId = _configuration.ClientId,
            ClientSecret = _configuration.ClientSecret
        });
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new ApplicationException($"Unable to revoke tokens. OIDC server responded {response.HttpStatusCode}:" +
                                           $" \r\n{response.Raw}");
        }
        
        _logger.LogInformation($"null [{DateTime.Now:s}] TraceId: {traceIdentifier} null "+
                               $"\"Token revoked.");
    }

    public async Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress)
    {        
        // Determine redirect URL
        var logOutRedirectEndpoint = _configuration.PostLogoutRedirectEndpoint.StartsWith('/')
            ? _configuration.PostLogoutRedirectEndpoint
            : $"/{_configuration.PostLogoutRedirectEndpoint}";
        
        var redirectUrl = $"{baseAddress}{logOutRedirectEndpoint}";

        return await BuildEndSessionUri(idToken, redirectUrl);
    }
    
    protected virtual async Task<Uri> BuildEndSessionUri(string? idToken, string redirectUri)
    {        
        var openIdConfiguration = await GetDiscoveryDocument();

        var endSessionUrEndpoint = openIdConfiguration.end_session_endpoint;
        if (endSessionUrEndpoint == null)
        {
            throw new NotSupportedException($"Invalid OpenId configuration. OpenId Configuration MUST contain a value for end_session_ endpoint. (https://openid.net/specs/openid-connect-session-1_0-17.html#OPMetadata)");
        }

        var urlEncodedRedirectUri = HttpUtility.UrlEncode(redirectUri);
        var endSessionUrl  = $"{endSessionUrEndpoint}?id_token_hint={idToken}&post_logout_redirect_uri={urlEncodedRedirectUri}";
        return new Uri(endSessionUrl);
    }
    
    protected virtual async Task<DiscoveryDocument?> ObtainDiscoveryDocument(string endpointAddress)
    {
        var discoveryDocument = await _httpClient.GetDiscoveryDocumentAsync(endpointAddress);
        if (discoveryDocument == null)
        {
            return null;
        }

        return new DiscoveryDocument
        {
            authorization_endpoint = discoveryDocument.AuthorizeEndpoint,
            end_session_endpoint = discoveryDocument.EndSessionEndpoint,
            issuer = discoveryDocument.Issuer,
            jwks_uri = discoveryDocument.JwksUri,
            revocation_endpoint = discoveryDocument.RevocationEndpoint,
            token_endpoint = discoveryDocument.TokenEndpoint,
            userinfo_endpoint = discoveryDocument.UserInfoEndpoint
        };
    }

    private async Task<DiscoveryDocument> GetDiscoveryDocument()
    {
        var endpointAddress = DiscoveryEndpointAddress;

        if (_cache.TryGetValue(DiscoveryEndpointAddress, out var discoveryDocument))
        {
            return (DiscoveryDocument)discoveryDocument;
        }
        
        discoveryDocument = await ObtainDiscoveryDocument(endpointAddress);

        if (discoveryDocument == null)
        {
            throw new ApplicationException(
                "Unable to login. Unable to find a well-known/openid-configuration document " +
                $"at {endpointAddress}");
        }

        _cache.Set(endpointAddress, discoveryDocument, TimeSpan.FromHours(1));
        return (DiscoveryDocument)discoveryDocument;
    }
}