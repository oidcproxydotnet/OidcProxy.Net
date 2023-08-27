using GoCloudNative.Bff.Authentication.ModuleInitializers;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public class OidcBffConfig : BffConfig
{
    public OpenIdConnectConfig Oidc { get; set; }
}