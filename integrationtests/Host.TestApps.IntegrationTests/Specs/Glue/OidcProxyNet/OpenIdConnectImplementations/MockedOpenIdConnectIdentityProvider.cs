using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet.OpenIdConnectImplementations;

public class MockedOpenIdConnectIdentityProvider(MockedOpenIdConnectIdentityProviderSettings settings, 
    ILogger logger, 
    IMemoryCache cache, 
    HttpClient httpClient, 
    OpenIdConnectConfig configuration) 
    : OpenIdConnectIdentityProvider(logger, cache, httpClient, configuration)
{
    public static bool HasRefreshedToken { get; set; } = false;

    public override async Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier, string traceIdentifier)
    {
        var token = await base.GetTokenAsync(redirectUri, code, codeVerifier, traceIdentifier);

        var accessToken = settings.TamperedPayload
            ? MimicTamperedAccessToken(token, settings)
            : token.access_token;

        var expiryDate = settings.WithExpiredToken
            ? DateTime.Now.AddDays(-1)
            : token.ExpiryDate;

        return new TokenResponse(accessToken,
            token.id_token, 
            token.refresh_token, 
            expiryDate);
    }

    public override async Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier)
    {
        var token = await base.RefreshTokenAsync(refreshToken, traceIdentifier);
        
        var accessToken = settings.TamperedPayload
            ? MimicTamperedAccessToken(token, settings)
            : token.access_token;

        HasRefreshedToken = true;

        return new TokenResponse(accessToken,
            token.id_token, 
            token.refresh_token, 
            token.ExpiryDate);
    }

    private static string MimicTamperedAccessToken(TokenResponse token, MockedOpenIdConnectIdentityProviderSettings settings)
    {
        var tokenParts = token?.access_token?.Split('.') ?? Array.Empty<string>();
        if (tokenParts.Length != 3)
        {
            Console.WriteLine("Invalid JWT token format.");
            return token.access_token;
        }

        var header = tokenParts[0];
        var payload = tokenParts[1];
        var signature = tokenParts[2];

        if (settings.TamperedPayload)
        {
            var decodedPayload = Base64UrlDecode(payload);
            var payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedPayload)
                ?? new Dictionary<string, object>();
            
            payloadDict["tampered"] = "true";
            payload = Base64UrlEncode(JsonConvert.SerializeObject(payloadDict));
        }

        if (settings.AlgorithmChanged)
        {
            var decodedPayload = Base64UrlDecode(header);
            var payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedPayload)
                              ?? new Dictionary<string, object>();
            
            payloadDict["alg"] = "FOO";
            header = Base64UrlEncode(JsonConvert.SerializeObject(payloadDict));
        }
        
        if (settings.WithNoHeader)
        {
            header = string.Empty;
        }

        return settings.WithTrailingDots 
            ? $"{header}.{payload}.{signature}.e30.e30"
            : $"{header}.{payload}.{signature}";
    }

    private static string Base64UrlEncode(string input)
    {
        var bytesToEncode = Encoding.UTF8.GetBytes(input);
        var encodedBytes = Convert.ToBase64String(bytesToEncode);
        return encodedBytes.Replace('+', '-').Replace('/', '_').TrimEnd('=');
    }

    private static string Base64UrlDecode(string input)
    {
        var paddedInput = input.Length % 4 == 0 
            ? input 
            : input + new string('=', 4 - input.Length % 4);
        
        var encodedBytes = paddedInput.Replace('-', '+').Replace('_', '/');
        var bytes = Convert.FromBase64String(encodedBytes);
        return Encoding.UTF8.GetString(bytes);
    }
}