using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;

namespace GoCloudNative.Bff.Authentication;

internal static class JwtParser
{
    public static JwtHeader ParseJwtHeader(this string token)
    {
        var middleSection = GetSection(token, 1);
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(middleSection));
        return JwtHeader.Deserialize(json);
    }
    
    public static JwtPayload ParseJwtPayload(this string token)
    {
        var middleSection = GetSection(token, 2);
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(middleSection));
        return JwtPayload.Deserialize(json);
    }

    private static string GetSection(string token, int section)
    {
        var match = Regex.Match(token, @"(\w*).(\w*).(\w*)");
        if (!match.Success)
        {
            throw new NotSupportedException($"Invalid token: {token}");
        }

        return match.Groups[section].Value;
    }
}