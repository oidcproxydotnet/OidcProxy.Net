using System.IdentityModel.Tokens.Jwt;
using System.Security.Authentication;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

public sealed class JweParser(ProxyOptions options, IJweEncryptionKey encryptionKey) : JwtParser(options)
{
    public override JwtPayload? ParseJwtPayload(string encryptedAccessToken)
    {
        try
        {
            var accessToken = encryptionKey.Decrypt(encryptedAccessToken);
            return base.ParseJwtPayload(accessToken);
        }
        catch (Exception e)
        {
            throw new AuthenticationException("Authentication failed. Unable to decrypt JWE. Use the " +
                                              "options.ConfigureJweParser(..) method to configure JWE handling" +
                                              $"correctly or implement the '{typeof(ITokenParser)}' class.", e);
        }
    }
}