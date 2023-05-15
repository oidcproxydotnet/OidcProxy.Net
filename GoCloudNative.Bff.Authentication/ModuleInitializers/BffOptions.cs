using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    public Action<IServiceCollection> IdentityProviderFactory = 
        (_) => throw new Exception("Unable to start. You must configure an identity provider. ");

    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    internal Action<IServiceCollection> ApplyDistributedCache => (s) => s.AddDistributedMemoryCache();
    
    public void LoadYarpFromConfig(IConfigurationSection configurationSection)
    {
        ApplyReverseProxyConfiguration = b => b.LoadFromConfig(configurationSection);
    }

    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        ApplyReverseProxyConfiguration = configuration;
    }
}