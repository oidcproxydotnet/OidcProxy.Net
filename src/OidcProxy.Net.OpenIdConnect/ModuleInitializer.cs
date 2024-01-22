using OidcProxy.Net.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.OpenIdConnect;

public static class ModuleInitializer
{
    public static void ConfigureOpenIdConnect(this ProxyOptions options, IConfigurationSection configurationSection, string endpointName = ".auth")
        => ConfigureOpenIdConnect(options, configurationSection.Get<OpenIdConnectConfig>(), endpointName);

    public static void ConfigureOpenIdConnect(this ProxyOptions options, OpenIdConnectConfig config, string endpointName = ".auth")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }

        options.RegisterIdentityProvider<OpenIdConnectIdentityProvider, OpenIdConnectConfig>(config, endpointName);
    }

    /// <summary>
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection, OidcProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}`.");
        }

        var oidcConfig = config.Oidc;
        var endpointName = config.EndpointName ?? ".auth";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();
        
        if (oidcConfig == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.Oidc)}.");
        }
        
        if (routes == null || !routes.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
        }
        
        if (clusters == null || !clusters.Any())
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
        }

        return serviceCollection.AddOidcProxy(options =>
        {
            AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
            AssignIfNotNull(config.LandingPage, options.SetLandingPage);
            AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
            AssignIfNotNull(config.CookieName, cookieName => options.CookieName = cookieName);
            
            options.EnableUserPreferredLandingPages = config.EnableUserPreferredLandingPages;
            options.SetAllowedLandingPages(config.AllowedLandingPages);
            
            if (config.SessionIdleTimeout.HasValue)
            {
                options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
            }
            
            options.ConfigureOpenIdConnect(oidcConfig, endpointName);
        
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