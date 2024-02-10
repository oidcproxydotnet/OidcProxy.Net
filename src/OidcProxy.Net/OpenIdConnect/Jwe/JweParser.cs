using System.IdentityModel.Tokens.Jwt;
using System.Text;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect.Jwe;

public class JweParser : JwtParser
{
    private readonly ITokenEncryptionKey _encryptionKey;

    public JweParser(ProxyOptions options, ITokenEncryptionKey encryptionKey) : base(options)
    {
        _encryptionKey = encryptionKey;
    }

    public override JwtPayload? ParseAccessToken(string accessToken) => null;

    public override Task<JwtPayload?> ParseAccessTokenAsync(string accessToken)
    {
        var plainText = _encryptionKey.Decrypt(accessToken);
        var payload = ParseJwtPayload(plainText);
        return Task.FromResult(payload);
    }
    
    private static JwtPayload? ParseJwtPayload(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return null;
        }

        var urlEncodedMiddleSection = GetSection(token, 1);
        
        var middleSection = urlEncodedMiddleSection
            .Replace('-', '+')
            .Replace('_', '/');
        
        var base64 = middleSection
            .PadRight(middleSection.Length + (4 - middleSection.Length % 4) % 4, '=');
        
        var bytes = Convert.FromBase64String(base64);
        var json = Encoding.UTF8.GetString(bytes);
        
        return string.IsNullOrEmpty(json)
            ? null
            : JwtPayload.Deserialize(json);
    }

    private static string GetSection(string token, int section)
    {
        var chunks = token.Split(".");
        if (chunks.Length != 3)
        {
            throw new NotSupportedException($"Invalid token: {token}");
        }

        return chunks[section];
    }
}