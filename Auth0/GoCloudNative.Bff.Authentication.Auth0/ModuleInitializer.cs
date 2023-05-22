using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.Auth0;

public static class ModuleInitializer
{
    public static void ConfigureAuth0(this BffOptions options, IConfigurationSection configurationSection)
        => ConfigureAuth0(options, configurationSection.Get<Auth0Config>());
    
    public static void ConfigureAuth0(this BffOptions options, Auth0Config config)
    {
        options.IdentityProviderFactory = (serviceCollection) =>
        {
            serviceCollection.AddTransient(_ => config);
            serviceCollection.AddHttpClient<IIdentityProvider, Auth0IdentityProvider>();
        };
    }
}