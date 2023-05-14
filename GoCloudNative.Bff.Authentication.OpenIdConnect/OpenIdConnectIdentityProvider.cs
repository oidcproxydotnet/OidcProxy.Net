using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Identity.Client;
using GoCloudNative.Bff.Authentication.IdentityProviders;

namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

internal class OpenIdConnectIdentityProvider : IIdentityProvider
{
    private readonly OpenIdConnectConfig _configuration;

    private static IConfidentialClientApplication CreateConfidentialClientApplication(OpenIdConnectConfig config, HttpContext httpContext)
    {   
        var protocol = httpContext.Request.IsHttps ? "https://" : "http://";
        var redirectUrl = $"{protocol}{httpContext.Request.Host}/login/callback";
        
        return ConfidentialClientApplicationBuilder.Create(config.ClientId)
            .WithExperimentalFeatures()
            .WithClientSecret(config.ClientSecret)
            .WithGenericAuthority(config.Authority)
            .WithRedirectUri(redirectUrl)
            .Build();
    }

    public OpenIdConnectIdentityProvider(OpenIdConnectConfig configuration)
    {
        _configuration = configuration;
    }
    
    public async Task<AuthorizeRequest> GetAuthorizeUrlAsync(HttpContext context)
    {   
        var appToAuthorize = CreateConfidentialClientApplication(_configuration, context);
        
        var authorizeUrl = await appToAuthorize.GetAuthorizationRequestUrl(_configuration.Scopes)
            .WithPkce(out var verifier)
            .ExecuteAsync();

        return new AuthorizeRequest(new Uri(authorizeUrl.ToString()), verifier);
    }

    public async Task<TokenResponse> GetTokenAsync(HttpContext context, string codeVerifier)
    {
        var code = context.Request.Query["code"].SingleOrDefault();
        if (string.IsNullOrEmpty(code))
        {
            throw new NotSupportedException("Cannot obtain a token without a code.");
        }

        var appToAuthorize = CreateConfidentialClientApplication(_configuration, context);

        var token = await appToAuthorize
            .AcquireTokenByAuthorizationCode(_configuration.Scopes, code)
            .WithPkceCodeVerifier(codeVerifier)
            .ExecuteAsync();

        return new TokenResponse(token.AccessToken, token.IdToken, null);
    }
}