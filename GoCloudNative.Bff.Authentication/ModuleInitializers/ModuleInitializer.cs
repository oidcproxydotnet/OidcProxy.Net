using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public static class ModuleInitializer
{
    private static readonly BffOptions _options = new();
    
    public static IServiceCollection AddSecurityBff(this IServiceCollection serviceCollection, 
        Action<BffOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(_options);
        
        var proxyBuilder = serviceCollection
            .AddReverseProxy();

        _options?.ApplyReverseProxyConfiguration(proxyBuilder);
        foreach (var proxyConfiguration in _options?.ProxyConfigurations)
        {
            proxyConfiguration.Invoke(proxyBuilder);
        }

        foreach (var idpRegistration in _options?.IdpRegistrations)
        {
            idpRegistration.Invoke(serviceCollection);
        }
        
        _options?.ApplyDistributedCache(serviceCollection);

        return serviceCollection
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
    }

    public static WebApplication UseSecurityBff(this WebApplication app)
    {
        foreach (var endpointRegistration in _options.IdpEndpointRegistrations)
        {
            endpointRegistration.Invoke(app);
        }
        
        app.MapReverseProxy();
        
        app.UseSession();

        return app;
    }
}