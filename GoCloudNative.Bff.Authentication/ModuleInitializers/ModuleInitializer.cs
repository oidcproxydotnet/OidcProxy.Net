using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public static class ModuleInitializer
{
    public static IServiceCollection AddSecurityBff(this IServiceCollection serviceCollection, 
        Action<BffOptions>? configureOptions = null)
    {
        var options = new BffOptions();
        configureOptions?.Invoke(options);
        
        var proxyBuilder = serviceCollection
            .AddReverseProxy()
            .AddTransforms<AddTokenHeaderTransferProvider>();

        options?.ApplyReverseProxyConfiguration(proxyBuilder);
        
        serviceCollection.AddDistributedMemoryCache(options?.ApplyDistributedCacheConfiguration ?? (_ => { }) );

        serviceCollection.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(15);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });

        serviceCollection
            .AddMemoryCache()
            .AddTransient(_ => options?.IdentityProviderFactory() 
                              ?? throw new NotSupportedException(
                                  "Unable to configure. IdentityProviderFactory cannot returm null."));

        return serviceCollection;
    }

    public static WebApplication UseSecurityBff(this WebApplication app)
    {
        app.MapAuthenticationEndpoints();
        app.MapReverseProxy();
        
        app.UseSession();

        return app;
    }
}