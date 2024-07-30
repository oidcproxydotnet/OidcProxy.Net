using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Auth0;

public static class ModuleInitializer
{    
    /// <summary>
    /// Initializes the proxy.
    /// </summary>
    /// <param name="serviceCollection">An instance of the IServiceCollection, used to register the classes OidcProxy.Net uses internally.</param>
    /// <param name="config">An object containing the values from AppSettings.config</param>
    /// <param name="configureOptions">A lambda, used to configure OidcProxy.</param>
    /// <returns>Used for chaining: Returns the instance of IServiceCollection.</returns>
    public static IServiceCollection AddAuth0Proxy(this IServiceCollection serviceCollection, Auth0ProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}`.");
        }

        if (config.Auth0 == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(Auth0ProxyConfig)}` " +
                $"and provide a value for {nameof(Auth0ProxyConfig)}.{nameof(config.Auth0)}.");
        }

        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }
        
        return serviceCollection
            .AddSingleton(config.Auth0)
            .AddOidcProxy<Auth0IdentityProvider>(options =>
            {
                config.Apply(options);
                configureOptions?.Invoke(options);
            });
    }

    public static void UseAuth0Proxy(this WebApplication app) => app.UseOidcProxy();
}