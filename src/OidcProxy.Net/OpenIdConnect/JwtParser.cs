using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace OidcProxy.Net.OpenIdConnect;

internal class JwtParser : IJwtParser
{
    public JwtPayload? ParseAccessToken(string accessToken) => ParseJwtPayload(accessToken);

    public JwtPayload? ParseIdToken(string idToken) => ParseJwtPayload(idToken);

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