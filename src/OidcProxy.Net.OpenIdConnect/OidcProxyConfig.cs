namespace OidcProxy.Net.OpenIdConnect;

public class OidcProxyConfig : ProxyConfig
{
    public OpenIdConnectConfig Oidc { get; set; } = new();

    public override bool Validate(out IEnumerable<string> errors)
    {
        var hasProxyConfig = base.Validate(out var baseErrors);
        var hasValidOidcConfig = Oidc.Validate(out var oidcErrors);

        errors = baseErrors.Union(oidcErrors);

        return hasProxyConfig && hasValidOidcConfig;
    }
}