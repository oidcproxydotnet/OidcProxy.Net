using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.IdentityProviders;

namespace OidcProxy.Net.ModuleInitializers;

public static class ModuleInitializer
{
    private static ProxyOptions Options = new();

    internal static void Reset() // Do not remove: Used for integration testing...
    {
        Options = new ProxyOptions();
    }

    /// <summary>
    /// Initializes the proxy.
    /// </summary>
    /// <param name="serviceCollection">An instance of the IServiceCollection, used to register the classes OidcProxy.Net uses internally.</param>
    /// <param name="configureOptions">A lambda, used to configure OidcProxy.</param>
    /// <returns>Used for chaining: Returns the instance of IServiceCollection.</returns>
    [Obsolete("Use `AddOidcProxy<TIdentityProvider>(options => {  })` instead.")]
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection,
        Action<ProxyOptions>? configureOptions = null)
    {
        // Apply all configuration
        configureOptions?.Invoke(Options);  
        
        // Bootstrap everything
        var configuration = Options.GetConfiguration();
        foreach (var option in configuration)
        {
            option.Configure(Options, serviceCollection);
        }
        
        return serviceCollection;
    }
    
    /// <summary>
    /// Initializes the proxy.
    /// </summary>
    /// <param name="serviceCollection">An instance of the IServiceCollection, used to register the classes OidcProxy.Net uses internally.</param>
    /// <param name="configureOptions">A lambda, used to configure OidcProxy.</param>
    /// <typeparam name="TIdentityProvider">A type reference to the concrete implementation of IIdentityProvider. This class implements everything that is needed to obtain tokens.</typeparam>
    /// <returns>For chaining purposes: Returns the instance of IServiceCollection.</returns>
    public static IServiceCollection AddOidcProxy<TIdentityProvider>(this IServiceCollection serviceCollection,
        Action<ProxyOptions>? configureOptions = null)
        where TIdentityProvider : class, IIdentityProvider
    {
        return AddOidcProxy<TIdentityProvider, DefaultAppSettingsSection>(serviceCollection, 
            new DefaultAppSettingsSection(),
            configureOptions);
    }

    /// <summary>
    /// Initializes the proxy.
    /// </summary>
    /// <param name="serviceCollection">An instance of the IServiceCollection, used to register the classes OidcProxy.Net uses internally.</param>
    /// <param name="appSettingsSection">The object that contains the values that have been defined in AppSettings.json</param>
    /// <param name="configureOptions">A lambda, used to configure OidcProxy.</param>
    /// <typeparam name="TIdentityProvider">A type reference to the concrete implementation of IIdentityProvider. This class implements everything that is needed to obtain tokens.</typeparam>
    /// <typeparam name="TAppSettingsSection">A type reference to the concrete implementation of IAppSettingsSection</typeparam>
    /// <returns>For chaining purposes: Returns the instance of IServiceCollection.</returns>
    public static IServiceCollection AddOidcProxy<TIdentityProvider, TAppSettingsSection>(
        this IServiceCollection serviceCollection,
        TAppSettingsSection appSettingsSection,
        Action<ProxyOptions>? configureOptions = null)
        where TIdentityProvider : class, IIdentityProvider
        where TAppSettingsSection : class, IAppSettingsSection
    {
        // Apply all configuration
        appSettingsSection.Apply(Options);
        configureOptions?.Invoke(Options);  
        
        // Register the IdentityProvider
        Options.RegisterIdentityProvider<TIdentityProvider, TAppSettingsSection>(appSettingsSection);
        
        // Bootstrap everything
        var configuration = Options.GetConfiguration();
        foreach (var option in configuration)
        {
            option.Configure(Options, serviceCollection);
        }
        
        return serviceCollection;
    }

    /// <summary>
    /// Bootstraps the proxy.
    /// </summary>
    public static WebApplication UseOidcProxy(this WebApplication app)
    {
        var configuration = Options.GetConfiguration();
        foreach (var option in configuration)
        {
            option.Configure(Options, app);
        }

        return app;
    }
}