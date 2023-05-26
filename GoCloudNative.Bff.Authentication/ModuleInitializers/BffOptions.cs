using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    internal readonly IdpRegistrations IdpRegistrations = new();

    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    internal Action<IServiceCollection> ApplyDistributedCache => (s) => s.AddDistributedMemoryCache();

    public void RegisterIdentityProvider<TIdentityProvider, TOptions>(TOptions options, string endpointName = "account") 
        where TIdentityProvider : class, IIdentityProvider 
        where TOptions : class
    {
        IdpRegistrations.Register<TIdentityProvider, TOptions>(options, endpointName);
    }

    public void LoadYarpFromConfig(IConfigurationSection configurationSection)
    {
        ApplyReverseProxyConfiguration = b => b.LoadFromConfig(configurationSection);
    }

    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        ApplyReverseProxyConfiguration = configuration;
    }
}