using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Auth0;

public class Auth0BffConfig : BffConfig
{
    public Auth0Config Auth0 { get; set; }
}