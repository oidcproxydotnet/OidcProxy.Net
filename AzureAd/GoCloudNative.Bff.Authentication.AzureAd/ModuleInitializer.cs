using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public static class ModuleInitializer
{
    public static void ConfigureAzureAd(this BffOptions options, IConfigurationSection configurationSection, string endpointName = "account")
        => ConfigureAzureAd(options, configurationSection.Get<AzureAdConfig>(), endpointName);

    public static void ConfigureAzureAd(this BffOptions options, AzureAdConfig config, string endpointName = "account")
    {
        options.RegisterIdentityProvider<AzureAdIdentityProvider, AzureAdConfig>(config, endpointName);
    }
}
