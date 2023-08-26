using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public static class ModuleInitializer
{
    private static readonly BffOptions Options = new();

    [Obsolete("Will be removed. Has been renamed to services.AddBff(o => {  }). Migrate to " +
              "services.AddBff(config, o => { }) for an easier way to configure the bff.")]
    public static IServiceCollection AddSecurityBff(this IServiceCollection serviceCollection,
        Action<BffOptions>? configureOptions = null) => AddBff(serviceCollection, configureOptions);
    
    /// <summary>
    /// Initialises the BFF
    /// </summary>
    public static IServiceCollection AddBff(this IServiceCollection serviceCollection,
        Action<BffOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);
        Options.Apply(serviceCollection);
        return serviceCollection;
    }

    /// <summary>
    /// Bootstraps the BFF.
    /// </summary>
    public static WebApplication UseBff(this WebApplication app)
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

    [Obsolete("Will be removed. Migrate to app.UseBff()")]
    public static WebApplication UseSecurityBff(this WebApplication app) => UseBff(app);
}