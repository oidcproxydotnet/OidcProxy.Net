using System.IdentityModel.Tokens.Jwt;
using System.Text;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Jwt;

public class JwtParser(ProxyOptions options) : ITokenParser
{
    public virtual string GetNameClaim() => options.NameClaim;
    
    public virtual string GetRoleClaim() => options.RoleClaim;

    public bool IsJwe(string token)
    {
        if (string.IsNullOrEmpty(token))
        {
            return false;
        }

        return token.Split('.', StringSplitOptions.RemoveEmptyEntries).Length == 5;
    }

    public virtual JwtPayload? ParseIdToken(string idToken) => ParsePayload(idToken);

    public virtual JwtPayload? ParseJwtPayload(string token) => ParsePayload(token);

    public JwtHeader? ParseJwtHeader(string token)
    {
        var parts = token.Split('.', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length != 3 && parts.Length != 5 // A JWT has 3 parts, a JWE has 5.
            ? null 
            : JwtHeader.Base64UrlDeserialize(parts[0]);
    }

    private static JwtPayload? ParsePayload(string token)
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
        var json = Decode(token);
        
        return string.IsNullOrEmpty(json)
            ? null
            : JwtPayload.Deserialize(json);
    }

    private static string Decode(string token)
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
        return Encoding.UTF8.GetString(bytes);
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