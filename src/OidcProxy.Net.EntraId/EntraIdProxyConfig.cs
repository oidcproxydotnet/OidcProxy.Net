using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.EntraId;

public class EntraIdProxyConfig : ProxyConfig
{
    public EntraIdConfig EntraId { get; set; }

    public override bool Validate(out IEnumerable<string> errors)
    {
        var hasProxyConfig = base.Validate(out var baseErrors);
        var hasValidOidcConfig = EntraId.Validate(out var oidcErrors);

        errors = baseErrors.Union(oidcErrors);

        return hasProxyConfig && hasValidOidcConfig;
    }
}