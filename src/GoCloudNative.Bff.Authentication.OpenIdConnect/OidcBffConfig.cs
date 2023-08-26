namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public class OidcBffConfig : ModuleInitializers.BffConfig
{
    public string? EndpointName { get; set; }
    public OpenIdConnectConfig Oidc { get; set; }
}