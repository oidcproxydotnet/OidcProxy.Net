using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public static class ModuleInitializer
{
    private static readonly BffOptions Options = new();
    
    public static IServiceCollection AddSecurityBff(this IServiceCollection serviceCollection, 
        Action<BffOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);
        
        var proxyBuilder = serviceCollection
            .AddTransient(_ => Options)
            .AddTransient<IRedirectUriFactory, RedirectUriFactory>()
            .AddReverseProxy();

        Options.ApplyReverseProxyConfiguration(proxyBuilder);
        
        Options.IdpRegistrations.Apply(proxyBuilder);
        
        Options.IdpRegistrations.Apply(serviceCollection);

        Options.ApplyClaimsTransformation(serviceCollection);
        
        return serviceCollection
            .AddDistributedMemoryCache()
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = Options.SessionIdleTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;    
                options.Cookie.Name = Options.SessionCookieName;
            });
    }

    public static WebApplication UseSecurityBff(this WebApplication app)
    {
        Options.IdpRegistrations.Apply(app);
        
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