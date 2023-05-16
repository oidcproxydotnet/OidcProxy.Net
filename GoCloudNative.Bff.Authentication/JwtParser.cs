using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System.Text.RegularExpressions;

namespace GoCloudNative.Bff.Authentication;

public static class JwtParser
{
    public static JwtPayload ParseJwt(this string token)
    {
        var match = Regex.Match(token, @"(\w*).(\w*).(\w*)");
        if (!match.Success)
        {
            throw new NotSupportedException($"Invalid token: {token}");
        }

        var middleSection = match.Groups[2].Value;
        var json = Encoding.UTF8.GetString(Convert.FromBase64String(middleSection));

        return JwtPayload.Deserialize(json);
    }
}