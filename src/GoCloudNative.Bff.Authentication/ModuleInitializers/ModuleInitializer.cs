using GoCloudNative.Bff.Authentication.OpenIdConnect;
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
            .AddTransient(_ => _options)
            .AddTransient<IRedirectUriFactory, RedirectUriFactory>()
            .AddReverseProxy();

        _options.ApplyReverseProxyConfiguration(proxyBuilder);
        
        _options.IdpRegistrations.Apply(proxyBuilder);
        
        _options.IdpRegistrations.Apply(serviceCollection);

        _options.ApplyClaimsTransformation(serviceCollection);
        
        return serviceCollection
            .AddDistributedMemoryCache()
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(15);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;    
                options.Cookie.Name = _options.SessionCookieName;
            });
    }

    public static WebApplication UseSecurityBff(this WebApplication app)
    {
        _options.IdpRegistrations.Apply(app);
        
        app.MapReverseProxy();
        
        app.UseSession();
        
        app.Use(async (context, next) =>
        {
            await context.Session.LoadAsync();
            await next();
        });

        return app;
    }
}