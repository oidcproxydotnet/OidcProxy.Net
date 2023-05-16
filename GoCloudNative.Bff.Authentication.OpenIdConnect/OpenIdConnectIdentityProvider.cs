using System.Net;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using IdentityModel;
using IdentityModel.Client;
using IdentityModel.OidcClient;
using TokenResponse = GoCloudNative.Bff.Authentication.IdentityProviders.TokenResponse;

namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

public class OpenIdConnectIdentityProvider : IIdentityProvider
{
    private readonly HttpClient _httpClient;
    
    private readonly OpenIdConnectConfig _configuration;
    
    public OpenIdConnectIdentityProvider(HttpClient httpClient, OpenIdConnectConfig configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }
    
    public virtual async Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context)
    { 
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{context.Request.Host}/oidc/login/callback";

        var client = new OidcClient(new OidcClientOptions
        {
            Authority = _configuration.Authority,
            ClientId = _configuration.ClientId,
            ClientSecret = _configuration.ClientSecret,
            RedirectUri = redirectUrl,
            Scope = string.Join(' ', _configuration.Scopes)
        });

        var request = await client.PrepareLoginAsync();
        
        return new AuthorizeRequest(new Uri(request.StartUrl), request.CodeVerifier);
    }

    public virtual async Task<TokenResponse> GetTokenAsync(HttpContext context, string code, string? codeVerifier)
    {
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{context.Request.Host}/oidc/login/callback";

        _httpClient.BaseAddress = new Uri("https://localhost:7031/connect/token");
        
        var response = await _httpClient.RequestTokenAsync(new AuthorizationCodeTokenRequest
        {
             GrantType = OidcConstants.GrantTypes.AuthorizationCode,
             ClientId = _configuration.ClientId,
             ClientSecret = _configuration.ClientSecret,
             
             Parameters =
             {
                 { "code", code },
                 { "scope", string.Join(' ', _configuration.Scopes) },
                 { "redirect_uri", redirectUrl },
                 { "code_verifier", codeVerifier },
             }
        });

        if (response.IsError)
        {
            throw new ApplicationException($"Unable to retrieve token. OIDC server responded: {response.Error}");
        }

        return new TokenResponse(response.AccessToken, response.IdentityToken, response.RefreshToken);
    }

    public virtual async Task Revoke(string accessToken)
    {
        var openIdConfiguration = await GetWellKnownConfiguration();
        
        var requestText = $"token={accessToken}" +
                          $"&client_id={_configuration.ClientId}" +
                          $"&client_secret={_configuration.ClientSecret}";
        
        var requestBody = new StringContent(requestText, Encoding.Default, "application/x-www-form-urlencoded");

        var response = await _httpClient.PostAsync(openIdConfiguration.revocation_endpoint, requestBody);
        if (response.StatusCode != HttpStatusCode.OK)
        {
            // Todo: implement logging here
            var responseBody = await response.Content.ReadAsStringAsync();
            throw new ApplicationException($"Unable to revoke tokens. OIDC server responded {response.StatusCode}:" +
                                           $" \r\n{responseBody}");
        }
    }
    
    private async Task<OpenIdConfiguration> GetWellKnownConfiguration()
    {
        var endpointAddress = $"{_configuration.Authority}{_configuration.WellKnownEndpoint}"
            .Replace("//", "/")
            .Replace("https:/", "https://");
        
        var httpResponse = await _httpClient.GetAsync(endpointAddress);
        return await httpResponse.Content.ReadFromJsonAsync<OpenIdConfiguration>();
    }
}