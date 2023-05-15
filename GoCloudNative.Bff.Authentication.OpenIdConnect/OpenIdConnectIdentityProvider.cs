using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Identity.Client;
using GoCloudNative.Bff.Authentication.IdentityProviders;

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
        var appToAuthorize = CreateConfidentialClientApplication(_configuration, context);
        
        var authorizeUrl = await appToAuthorize.GetAuthorizationRequestUrl(_configuration.Scopes)
            .WithPkce(out var verifier)
            .ExecuteAsync();

        return new AuthorizeRequest(new Uri(authorizeUrl.ToString()), verifier);
    }

    public virtual async Task<TokenResponse> GetTokenAsync(HttpContext context, string code, string? codeVerifier)
    {
        var appToAuthorize = CreateConfidentialClientApplication(_configuration, context);

        var token = await appToAuthorize
            .AcquireTokenByAuthorizationCode(_configuration.Scopes, code)
            .WithPkceCodeVerifier(codeVerifier)
            .ExecuteAsync();
        
        return new TokenResponse(token.AccessToken, token.IdToken, null);
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

    private static IConfidentialClientApplication CreateConfidentialClientApplication(OpenIdConnectConfig config, HttpContext httpContext)
    {   
        var protocol = httpContext.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{httpContext.Request.Host}/oidc/login/callback";
        
        return ConfidentialClientApplicationBuilder.Create(config.ClientId)
            .WithExperimentalFeatures()
            .WithClientSecret(config.ClientSecret)
            .WithGenericAuthority(config.Authority)
            .WithRedirectUri(redirectUrl)
            .Build();
    }
}