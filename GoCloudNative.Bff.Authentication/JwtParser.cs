using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace GoCloudNative.Bff.Authentication;

public static class JwtParser
{   
    public static JwtPayload ParseJwtPayload(this string token)
    {
        var middleSection = GetSection(token, 1);
        byte[] bytes;

        try
        {
            bytes = Convert.FromBase64String(middleSection);
        }
        catch (FormatException)
        {
            bytes = Convert.FromBase64String($"{middleSection}==");
        }
        
        var json = Encoding.UTF8.GetString(bytes);
        return JwtPayload.Deserialize(json);
    }

    private static string GetSection(string token, int section)
    {
        var chunks = token.Split(".", StringSplitOptions.None);
        if (chunks.Length != 3)
        {
            throw new NotSupportedException($"Invalid token: {token}");
        }

        return chunks[section];
    }
}