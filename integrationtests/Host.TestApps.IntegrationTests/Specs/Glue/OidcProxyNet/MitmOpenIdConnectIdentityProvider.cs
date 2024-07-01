using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Newtonsoft.Json;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

public class MitmOpenIdConnectIdentityProvider(ILogger logger, IMemoryCache cache, HttpClient httpClient, OpenIdConnectConfig configuration) 
    : OpenIdConnectIdentityProvider(logger, cache, httpClient, configuration)
{
    public override async Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier, string traceIdentifier)
    {
        var token = await base.GetTokenAsync(redirectUri, code, codeVerifier, traceIdentifier);

        var tokenParts = token.access_token.Split('.');
        if (tokenParts.Length != 3)
        {
            Console.WriteLine("Invalid JWT token format.");
            return token;
        }

        var header = tokenParts[0];
        var payload = tokenParts[1];
        var signature = tokenParts[2];

        var decodedPayload = Base64UrlDecode(payload);
        
        var payloadDict = JsonConvert.DeserializeObject<Dictionary<string, object>>(decodedPayload);
        payloadDict["tampered"] = "true";

        var newPayload = Base64UrlEncode(JsonConvert.SerializeObject(payloadDict));

        return new TokenResponse($"{header}.{newPayload}.{signature}",
            token.id_token, 
            token.refresh_token, 
            token.ExpiryDate);
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