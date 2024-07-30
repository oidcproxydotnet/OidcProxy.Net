using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

public static class ModuleInitializer
{
    /// <summary>
    /// Initializes the proxy.
    /// </summary>
    /// <param name="serviceCollection">An instance of the IServiceCollection, used to register the classes OidcProxy.Net uses internally.</param>
    /// <param name="config">An object containing the values from AppSettings.config</param>
    /// <param name="configureOptions">A lambda, used to configure OidcProxy.</param>
    /// <returns>Used for chaining: Returns the instance of IServiceCollection.</returns>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection, 
        OidcProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}`.");
        }
        
        if (config.Oidc == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                                        $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                                        $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.Oidc)}.");
        }
        
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }
        
        serviceCollection.AddSingleton(config.Oidc);
        
        return serviceCollection.AddOidcProxy<OpenIdConnectIdentityProvider>(options =>
        {
            config.Apply(options);
            configureOptions?.Invoke(options);
        });
    }
}