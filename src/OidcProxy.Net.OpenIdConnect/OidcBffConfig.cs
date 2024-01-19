using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

public class OidcBffConfig : BffConfig
{
    public OpenIdConnectConfig Oidc { get; set; }
}