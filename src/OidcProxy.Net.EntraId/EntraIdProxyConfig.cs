using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.EntraId;

public class EntraIdProxyConfig : ProxyConfig
{
    public EntraIdConfig EntraId { get; set; }
}