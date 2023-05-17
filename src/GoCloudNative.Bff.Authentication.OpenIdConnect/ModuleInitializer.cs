using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

public static class ModuleInitializer
{
    public static void ConfigureOpenIdConnect(this BffOptions options, IConfigurationSection configurationSection)
        => ConfigureOpenIdConnect(options, configurationSection.Get<OpenIdConnectConfig>());

    public static void ConfigureOpenIdConnect(this BffOptions options, OpenIdConnectConfig config)
    {
        options.IdentityProviderFactory = (serviceCollection) =>
        {
            serviceCollection.AddTransient(_ => config);

            serviceCollection.AddHttpClient<IIdentityProvider, OpenIdConnectIdentityProvider>();
        };
    }
}