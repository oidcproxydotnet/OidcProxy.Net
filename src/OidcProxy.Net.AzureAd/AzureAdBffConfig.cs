using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.AzureAd;

public class AzureAdBffConfig : BffConfig
{
    public AzureAdConfig AzureAd { get; set; }
}