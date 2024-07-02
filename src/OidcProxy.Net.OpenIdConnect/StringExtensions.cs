namespace OidcProxy.Net.OpenIdConnect;

internal static class StringExtensions
{
    public static byte[] Base64UrlDecode(this string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return Array.Empty<byte>();
        }

        var output = input.Replace('-', '+').Replace('_', '/');
        switch (output.Length % 4)
        {
            case 0: break;
            case 2: output += "=="; break;
            case 3: output += "="; break;
            default: throw new ArgumentException("Illegal base64url string.");
        }
    
        return Convert.FromBase64String(output);
    }
}