using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.ModuleInitializers;

public static class ModuleInitializer
{
    private static readonly BffOptions Options = new();

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
        if (Options.IdpRegistration == null)
        {
            throw new NotSupportedException("Cannot bootstrap GoCloudNative.Bff. Register an identity provider.");
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