using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;

namespace TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

public static class ModuleInitializer
{
    public static void ConfigureOpenIdConnect(this BffOptions options, IConfigurationSection configurationSection)
        => ConfigureOpenIdConnect(options, configurationSection.Get<OpenIdConnectConfig>());

    public static void ConfigureOpenIdConnect(this BffOptions options, OpenIdConnectConfig config)
    {
        options.IdentityProviderFactory = () => new OpenIdConnectIdentityProvider(config);
    }
}