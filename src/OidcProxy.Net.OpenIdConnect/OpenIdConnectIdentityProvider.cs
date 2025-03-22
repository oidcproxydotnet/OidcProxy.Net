using System.Net;
using System.Web;
using Duende.IdentityModel;
using Duende.IdentityModel.Client;
using Duende.IdentityModel.OidcClient;
using OidcProxy.Net.IdentityProviders;
using Microsoft.Extensions.Caching.Memory;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.Logging;
using TokenResponse = OidcProxy.Net.IdentityProviders.TokenResponse;

namespace OidcProxy.Net.OpenIdConnect;

public class OpenIdConnectIdentityProvider(
    ILogger logger,
    IMemoryCache cache,
    HttpClient httpClient,
    OpenIdConnectConfig configuration)
    : IIdentityProvider
{
    protected virtual string DiscoveryEndpointAddress 
        => $"{configuration.Authority.TrimEnd('/')}/" + $"{configuration.DiscoveryEndpoint.TrimStart('/')}";

    protected virtual Parameters? GetFrontChannelParameters() {
        return null;
    }

    public virtual async Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
    { 
        var scopes = new Scopes(configuration.Scopes);

        var client = new OidcClient(new OidcClientOptions
        {
            Authority = configuration.Authority,
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret,
            RedirectUri = redirectUri,
            Scope = string.Join(" ", scopes),
            DisablePushedAuthorization = configuration.DisablePushedAuthorization
        });

        var request = await client.PrepareLoginAsync(GetFrontChannelParameters());
        
        return new AuthorizeRequest(new Uri(request.StartUrl), request.CodeVerifier);
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
        
        var response = await httpClient.RequestTokenAsync(new AuthorizationCodeTokenRequest
        {
             Address = wellKnown.token_endpoint,
             GrantType = OidcConstants.GrantTypes.AuthorizationCode,
             ClientId = configuration.ClientId,
             ClientSecret = configuration.ClientSecret,
             
             Parameters =
             {
                 { OidcConstants.TokenRequest.Code, code },
                 { OidcConstants.TokenRequest.RedirectUri, redirectUri },
                 { OidcConstants.TokenRequest.CodeVerifier, codeVerifier },
             }
        });
        
        if (response.IsError)
        {
            throw new ApplicationException($"Unable to retrieve token. " +
                                           $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
        }

        await logger.InformAsync($"Queried /token endpoint and obtained id_, access_, and refresh_tokens.");
        
        var expiryDate = DateTime.UtcNow.AddSeconds(response.ExpiresIn);
        
        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiryDate);
    }

    public async Task<IEnumerable<KeySet>> GetJwksAsync(bool invalidateCache = false)
    {
        var openIdConfiguration = await GetDiscoveryDocument();
        var jwksUri = openIdConfiguration.jwks_uri;

        if (!invalidateCache && cache.TryGetValue(jwksUri, out var keySet) && keySet != null)
        {
            return (JsonWebKeySet)keySet;
        }
        
        var response = await httpClient.GetJsonWebKeySetAsync(jwksUri);
        if (response.IsError)
        {
            throw new ApplicationException($"Unable to JSON Web Key Set. " +
                                           $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
        }
        
        keySet = JsonWebKeySet.Create(response);
        cache.Set(jwksUri, keySet, TimeSpan.FromHours(1));
        return keySet as IEnumerable<KeySet> ?? Array.Empty<KeySet>();
    }

    public virtual async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier)
    {
        var openIdConfiguration = await GetDiscoveryDocument();
        var scopes = new Scopes(configuration.Scopes);

        var response = await httpClient.RequestRefreshTokenAsync(new RefreshTokenRequest
        {
            Address = openIdConfiguration.token_endpoint,
            GrantType = OidcConstants.GrantTypes.RefreshToken,
            RefreshToken = refreshToken,
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret,
            Scope = string.Join(' ', scopes)
        });
        
        if (response.IsError)
        {
            throw new TokenRenewalFailedException($"Unable to retrieve token. " +
                                                  $"OIDC server responded {response.HttpStatusCode}: {response.Raw}");
        }
        
        await logger.InformAsync($"Queried /token endpoint (refresh grant) and obtained id_, access_, and refresh_tokens.");

        var expiresIn = DateTime.UtcNow.AddSeconds(response.ExpiresIn);

        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken, expiresIn);
    }

    public virtual async Task RevokeAsync(string token, string traceIdentifier)
    {
        var openIdConfiguration = await GetDiscoveryDocument();

        var response = await httpClient.RevokeTokenAsync(new TokenRevocationRequest
        {
            Address = openIdConfiguration.revocation_endpoint,
            Token = token,
            ClientId = configuration.ClientId,
            ClientSecret = configuration.ClientSecret
        });
        
        if (response.HttpStatusCode != HttpStatusCode.OK)
        {
            throw new ApplicationException($"Unable to revoke tokens. OIDC server responded {response.HttpStatusCode}:" +
                                           $" \r\n{response.Raw}");
        }
        
        await logger.InformAsync($"Token revoked.");
    }

    public async Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress)
    {        
        // Determine redirect URL
        var logOutRedirectEndpoint = configuration.PostLogoutRedirectEndpoint.StartsWith('/')
            ? configuration.PostLogoutRedirectEndpoint
            : $"/{configuration.PostLogoutRedirectEndpoint}";
        
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
        var discoveryDocument = await httpClient.GetDiscoveryDocumentAsync(endpointAddress);
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

    public async Task<DiscoveryDocument> GetDiscoveryDocument()
    {
        var endpointAddress = DiscoveryEndpointAddress;

        if (cache.TryGetValue(DiscoveryEndpointAddress, out var discoveryDocument))
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

        cache.Set(endpointAddress, discoveryDocument, TimeSpan.FromHours(1));
        return (DiscoveryDocument)discoveryDocument;
    }
}