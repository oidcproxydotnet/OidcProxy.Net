namespace OidcProxy.Net.Auth0;

public class Auth0ProxyConfig : ProxyConfig
{
    public Auth0Config Auth0 { get; set; }

    public override bool Validate(out IEnumerable<string> errors)
    {
        var hasProxyConfig = base.Validate(out var baseErrors);
        var hasValidOidcConfig = Auth0.Validate(out var oidcErrors);

        errors = baseErrors.Union(oidcErrors);

        return hasProxyConfig && hasValidOidcConfig;
    }
}