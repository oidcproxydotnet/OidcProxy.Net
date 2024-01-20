using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.ModuleInitializers;

public static class ModuleInitializer
{
    private static readonly ProxyOptions Options = new();

    /// <summary>
    /// Initialises the proxy
    /// </summary>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection,
        Action<ProxyOptions>? configureOptions = null)
    {
        configureOptions?.Invoke(Options);
        
        Options.Apply(serviceCollection);
        return serviceCollection;
    }

    /// <summary>
    /// Bootstraps the proxy.
    /// </summary>
    public static WebApplication UseOidcProxy(this WebApplication app)
    {
        if (Options.IdpRegistration == null)
        {
            throw new NotSupportedException("Cannot bootstrap OidcProxy.Net. Register an identity provider.");
        }

        Options.IdpRegistration.Apply(app);

        app.UseSession();
        app.UseAuthentication();
        app.UseAuthorization();
        
        app.Use(async (context, next) =>
        {
            await context.Session.LoadAsync();
            await next();
        });

        return app;
    }
}