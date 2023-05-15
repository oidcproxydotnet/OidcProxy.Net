using Auth0.AuthenticationApi;
using Auth0.AuthenticationApi.Models;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using Vendor = Auth0;

namespace GoCloudNative.Bff.Authentication.Auth0;

public class Auth0IdentityProvider : IIdentityProvider
{
    private readonly Auth0Config _config;

    public Auth0IdentityProvider(Auth0Config config)
    {
        _config = config;
    }
    
    public Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context)
    {
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{context.Request.Host}/oidc/login/callback";
        
        var client = new Vendor.AuthenticationApi.AuthenticationApiClient(new Uri(_config.Authority));
        var authorizeUrl = client.BuildAuthorizationUrl()
            .WithClient(_config.ClientId)
            .WithScopes(_config.Scopes)
            .WithRedirectUrl(redirectUrl)
            .WithResponseType(AuthorizationResponseType.Code)
            .Build();

        return Task.FromResult(new AuthorizeRequest(authorizeUrl));
    }

    public async Task<TokenResponse> GetTokenAsync(HttpContext context, string code, string? codeVerifier)
    {
        var protocol = context.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{context.Request.Host}/oidc/login/callback";
        
        var client = CreateAuthenticationApiClient();
        var response = await client.GetTokenAsync(new AuthorizationCodeTokenRequest
        { 
            Code = code,
            ClientId = _config.ClientId,
            ClientSecret = _config.ClientSecret,
            RedirectUri = redirectUrl
        });

        return new TokenResponse(response.AccessToken, response.IdToken, response.RefreshToken);
    }

    public Task Revoke(string accessToken)
    {
        // I.l.e.: Auth0 does not support token revocation
        
        return Task.CompletedTask;
    }
    
    private AuthenticationApiClient CreateAuthenticationApiClient()
    {
        return new Vendor.AuthenticationApi.AuthenticationApiClient(new Uri(_config.Authority));
    }

}