using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace OidcProxy.Net.OpenIdConnect;

internal static class JwtParser
{   
    public static JwtPayload? ParseJwtPayload(this string token)
    {
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