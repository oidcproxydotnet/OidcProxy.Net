using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.AzureAd;

public static class ModuleInitializer
{
    public static void ConfigureAzureAd(this BffOptions options, IConfigurationSection configurationSection)
        => ConfigureAzureAd(options, configurationSection.Get<AzureAdConfig>());

    public static void ConfigureAzureAd(this BffOptions options, AzureAdConfig config)
    {
        options.IdentityProviderFactory = (serviceCollection) =>
        {
            serviceCollection.AddTransient(_ => config);

            serviceCollection.AddHttpClient<IIdentityProvider, AzureAdIdentityProvider>();
        };
    }
}
