using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

public class JweParser : JwtParser
{
    private readonly IJweEncryptionKey _encryptionKey;

    public JweParser(ProxyOptions options, IJweEncryptionKey encryptionKey) : base(options)
    {
        _encryptionKey = encryptionKey;
    }

    public override JwtPayload? ParseAccessToken(string encryptedAccessToken)
    {
        try
        {
            var accessToken = _encryptionKey.Decrypt(encryptedAccessToken);
            return base.ParseAccessToken(accessToken);
        }
        catch (Exception e)
        {
            throw new AuthenticationException("Authentication failed. Unable to decrypt JWE. Use the " +
                                              "options.ConfigureJweParser(..) method to configure JWE handling" +
                                              $"correctly or implement the '{typeof(ITokenParser)}' class.", e);
        }
    }
}