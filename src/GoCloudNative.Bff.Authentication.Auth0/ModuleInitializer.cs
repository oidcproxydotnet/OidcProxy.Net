using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.Auth0;

public static class ModuleInitializer
{
    public static void ConfigureAuth0(this BffOptions options, IConfigurationSection configurationSection, string endpointName = "account")
        => ConfigureAuth0(options, configurationSection.Get<Auth0Config>(), endpointName);
    
    public static void ConfigureAuth0(this BffOptions options, Auth0Config config, string endpointName = "account")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }
        
        options.RegisterIdentityProvider<Auth0IdentityProvider, Auth0Config>(config, endpointName);
    }
    
    /// <summary>
    /// Initialises the BFF. Also use app.UseBff();
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddBff(this IServiceCollection serviceCollection, Auth0BffConfig config,
        Action<BffOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise GoCloudNative.Authentication.Bff. Config cannot be null. " +
                $"Invoke `builder.Services.AddBff(..)` with an instance of `{nameof(Auth0BffConfig)}`.");
        }

        var auth0Config = config.Auth0;
        var endpointName = config.EndpointName ?? "account";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();
        
        if (auth0Config == null)
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddBff(..)` with an instance of `{nameof(Auth0BffConfig)}` " +
                $"and provide a value for {nameof(Auth0BffConfig)}.{nameof(config.Auth0)}.");
        }
        
        if (routes == null || !routes.Any())
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddBff(..)` with an instance of `{nameof(Auth0BffConfig)}` " +
                $"and provide a value for {nameof(Auth0BffConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
        }
        
        if (clusters == null || !clusters.Any())
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddBff(..)` with an instance of `{nameof(Auth0BffConfig)}` " +
                $"and provide a value for {nameof(Auth0BffConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
        }

        return serviceCollection.AddBff(options =>
        {
            AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
            AssignIfNotNull(config.LandingPage, options.SetLandingPage);
            AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
            AssignIfNotNull(config.SessionCookieName, cookieName => options.SessionCookieName = cookieName);
            
            if (config.SessionIdleTimeout.HasValue)
            {
                options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
            }
            
            options.ConfigureAuth0(auth0Config, endpointName);
        
            options.ConfigureYarp(yarp => yarp.LoadFromMemory(routes, clusters));
            
            configureOptions?.Invoke(options);
        });
    }
        
    private static void AssignIfNotNull<T>(T? value, Action<T> @do)
    {
        if (value != null)
        {
            @do(value);
        }
    }
}