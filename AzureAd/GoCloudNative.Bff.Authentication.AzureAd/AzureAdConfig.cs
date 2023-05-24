using GoCloudNative.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public class AzureAdConfig : OpenIdConnectConfig
{
    public AzureAdConfig()
    {
        this.DiscoveryEndpoint = "https://login.microsoftonline.com/common/v2.0/.well-known/openid-configuration";
    }

    public string TenantId { get; set; }
}