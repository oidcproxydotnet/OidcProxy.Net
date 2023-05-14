using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    public Func<IIdentityProvider> IdentityProviderFactory = 
        () => throw new Exception("Unable to start. You must configure an identity provider. ");
    
    internal Action<MemoryDistributedCacheOptions> ApplyDistributedCacheConfiguration = _ => { };

    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    public void LoadYarpFromConfig(IConfigurationSection configurationSection)
    {
        ApplyReverseProxyConfiguration = b => b.LoadFromConfig(configurationSection);
    }

    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        ApplyReverseProxyConfiguration = configuration;
    }
    
    public void ConfigureDistributedCache(Action<MemoryDistributedCacheOptions> configuration)
    {
        ApplyDistributedCacheConfiguration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }
}