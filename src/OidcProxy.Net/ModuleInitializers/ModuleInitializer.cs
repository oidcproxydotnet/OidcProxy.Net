using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.ModuleInitializers;

public static class ModuleInitializer
{
    private static ProxyOptions Options = new();

    internal static void Reset()
    {
        Options = new ProxyOptions();
    }

    /// <summary>
    /// Initialises the proxy
    /// </summary>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection,
        Action<ProxyOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);  
        
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