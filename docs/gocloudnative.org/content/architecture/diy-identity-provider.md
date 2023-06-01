# Connecting GoCloudNative.BFF with a custom OIDC/OAuth2 server

The GoCloudNative.Bff does not support all identity providers out of the box. Also, it does not encourage to use the `authorization code flow` without PKCE. 

The GoCloudNative.Bff only supports OpenId Connect compliant authentication servers, and Auth0, and Azure Active Directory.

Assume you want to use GitHub for an Identity Provider. Or Google. Or you have a legacy identity provider which only supports the `authorization code flow` without PKCE. Then what?

This documents describes how to implement an Identity Provider from scratch.

## Implementing the `IIdentityProvider` interface

At the core of the GoCloudNative.Bff, there is the `IIdentityProvider` interface. It implements the OAuth2 "flow":

![GoCloudNative.Bff authentication flow](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/auth_code-sequence-diagram.png)

This is reflected in the `IIdentityProvider` interface which has the following signature:

```csharp
public interface IIdentityProvider
{
    Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri);
    
    Task<TokenResponse> GetTokenAsync(string redirectUri, 
        string code, 
        string? codeVerifier);
    
    Task<TokenResponse> RefreshTokenAsync(string refreshToken);
    
    Task RevokeAsync(string token);
    
    Task<Uri> GetEndSessionEndpointAsync(string? idToken, 
        string baseAddress);
}
```

Assume you want to implement the `authorization code` flow to connect to a legacy OAuth2 authorization server. In that case, the implementation might look similar to this:

```csharp
using System.Text;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Newtonsoft.Json.Linq;

namespace YourApp;

public class LegacyServerConfig
{
    public string Authority { get; set; }
    public string ClientId { get; set; }
}

public class LegacyIdentityProvider : IIdentityProvider
{
    private readonly LegacyServerConfig _config;
    private readonly HttpClient _httpClient;

    public LegacyIdentityProvider(LegacyServerConfig config, HttpClient httpClient)
    {
        _config = config;
        _httpClient = httpClient;
    }
    
    // Create the authorize URI here as defined in the OAuth2 RFC (https://www.rfc-editor.org/rfc/rfc6749#section-4.1.1)
    public Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
    {
        const string uriFormat = "{0}/authorize?response_type=code&client_id={1}&state={2}&redirect_uri={3}&scope=openid+profile";
        var uri = string.Format(uriFormat, _config.Authority, _config.ClientId, "xyz", redirectUri);

        string? codeVerifier = null; // not applicable for legacy auth code flow

        var result = new AuthorizeRequest(new Uri(uri), codeVerifier);
        return Task.FromResult(result);
    }

    public async Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier)
    {
        _httpClient.DefaultRequestHeaders.Add("content-type", "application/x-www-form-urlencoded");

        const string requestBodyFormat = "grant_type=authorization_code&code={0}&client_id={1}&redirect_uri={2}";
        var requestBody = string.Format(requestBodyFormat, code, _config.ClientId, redirectUri);
        var requestContent = new StringContent(requestBody, Encoding.UTF8, "application/x-www-form-urlencoded");

        var requestUrl = $"{_config.Authority}/token";

        var response = await _httpClient.PostAsync(requestUrl, requestContent);
        var responseBody = await response.Content.ReadAsStringAsync();
        var responseJson = JObject.Parse(responseBody);

        return new TokenResponse(responseJson["access_token"].Value<string>(), 
            responseJson["id_token"].Value<string>(),
            responseJson["refresh_token"].Value<string>(), 
            DateTime.Now.AddSeconds(responseJson["expires_in"].Value<int>()));
    }

    public Task<TokenResponse> RefreshTokenAsync(string refreshToken)
    {
        // And so forth
        throw new NotImplementedException();
    }

    public Task RevokeAsync(string token)
    {
        // And so forth
        throw new NotImplementedException();
    }

    public Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress)
    {
        // And so forth
        throw new NotImplementedException();
    }
}
```

This code demonstrates how to implement an Identity Provider. As demonstrated in this example, using the IIdentityProvider interface, you can implement OAuth2 in any way that fits your need.

After you have completed your implementation of the `IIdentityProvider` interface, you must register it. You can do so by writing the following `Program.cs`:

```csharp

using GoCloudNative.Bff.Authentication.OpenIdConnect;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    var options = builder.Configuration.GetSection("LegacyServer").Get<LegacyServerConfig>();
    o.RegisterIdentityProvider<LegacyIdentityProvider, LegacyServerConfig>(options);
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();

```