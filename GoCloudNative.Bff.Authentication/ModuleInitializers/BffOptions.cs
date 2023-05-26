using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    internal readonly List<Action<IServiceCollection>> IdpRegistrations = new();
    
    internal readonly List<Action<WebApplication>> IdpEndpointRegistrations = new();

    internal readonly List<Action<IReverseProxyBuilder>> ProxyConfigurations = new();

    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    internal Action<IServiceCollection> ApplyDistributedCache => (s) => s.AddDistributedMemoryCache();

    public void RegisterIdentityProvider<TIdentityProvider, TOptions>(TOptions options, string endpointName = "account") 
        where TIdentityProvider : class, IIdentityProvider 
        where TOptions : class
    {
        IdpRegistrations.Add(s => s
            .AddTransient<TIdentityProvider>()
            .AddSingleton(_ => options)
            .AddHttpClient<TIdentityProvider>()
        );
        
        ProxyConfigurations.Add(c =>
        {
            c.AddTransforms<HttpHeaderTransformation<TIdentityProvider>>();
        });
        
        IdpEndpointRegistrations.Add(app => app.MapAuthenticationEndpoints<TIdentityProvider>(endpointName));
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