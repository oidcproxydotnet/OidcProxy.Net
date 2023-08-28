using GoCloudNative.Bff.Authentication.ModuleInitializers;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public class AzureAdBffConfig : BffConfig
{
    public AzureAdConfig AzureAd { get; set; }
}