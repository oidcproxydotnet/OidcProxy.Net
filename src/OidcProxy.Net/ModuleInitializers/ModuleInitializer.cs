using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Middleware;

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
        serviceCollection.AddTransient<AnonymousAccessMiddleware>(); // done
        
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

        app.UseAuthentication(); // done
        app.UseAuthorization(); // done
        
        if (!Options.AllowAnonymousAccess)
        {
            app.UseMiddleware<AnonymousAccessMiddleware>();
        } // done

        app.UseSession(); // done
        
        app.Use(async (context, next) =>
        {
            await context.Session.LoadAsync();
            await next();
        }); // done

        return app;
    }
}