using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;

namespace GoCloudNative.Bff.Authentication.Auth0;

public static class ModuleInitializer
{
    public static void ConfigureAuth0(this BffOptions options, IConfigurationSection configurationSection, string endpointName = "account")
        => ConfigureAuth0(options, configurationSection.Get<Auth0Config>(), endpointName);
    
    public static void ConfigureAuth0(this BffOptions options, Auth0Config config, string endpointName = "account")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }
        
        options.RegisterIdentityProvider<Auth0IdentityProvider, Auth0Config>(config, endpointName);
    }
}