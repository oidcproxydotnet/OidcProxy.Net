using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

public class OidcProxyConfig : ProxyConfig
{
    public OpenIdConnectConfig Oidc { get; set; }
}