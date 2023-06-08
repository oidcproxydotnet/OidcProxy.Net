using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    internal readonly IdpRegistrations IdpRegistrations = new();
    
    internal Uri? CustomHostName = null;

    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    internal Action<IServiceCollection> ApplyDistributedCache => (s) => s.AddDistributedMemoryCache();

    public string SessionCookieName { get; set; } = "bff.cookie";
    
    public bool AlwaysRedirectToHttps { get; set; } = true;

    public void SetCustomHostName(Uri hostname)
    {
        if (!string.IsNullOrEmpty(hostname.Query))
        {
            throw new NotSupportedException("GCN-B-322cf6ab8a70: " +
                                            "Cannot initialize GoCloudNative.BFF. " +
                                            "Error configuring custom hostname. " +
                                            $"{hostname} is not a valid hostname. " +
                                            "A custom hostname may not have a querystring.");
        }

        CustomHostName = hostname;
    }

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