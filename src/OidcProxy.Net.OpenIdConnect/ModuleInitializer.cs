using OidcProxy.Net.ModuleInitializers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.OpenIdConnect;

public static class ModuleInitializer
{
    public static void ConfigureOpenIdConnect(this ProxyOptions options, IConfigurationSection configurationSection, string endpointName = "account")
        => ConfigureOpenIdConnect(options, configurationSection.Get<OpenIdConnectConfig>(), endpointName);

    public static void ConfigureOpenIdConnect(this ProxyOptions options, OpenIdConnectConfig config, string endpointName = "account")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }

        options.RegisterIdentityProvider<OpenIdConnectIdentityProvider, OpenIdConnectConfig>(config, endpointName);
    }

    /// <summary>
    /// Initialises the BFF. Also use app.UseBff();
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddOidcProxy(this IServiceCollection serviceCollection, OidcProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config), "Failed to initialise GoCloudNative.Authentication.Bff. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}`.");
        }

        var oidcConfig = config.Oidc;
        var endpointName = config.EndpointName ?? "account";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();
        
        if (oidcConfig == null)
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.Oidc)}.");
        }
        
        if (routes == null || !routes.Any())
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Routes)}.");
        }
        
        if (clusters == null || !clusters.Any())
        {
            throw new ArgumentException("Failed to initialise GoCloudNative.Authentication.Bff. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(OidcProxyConfig)}` " +
                $"and provide a value for {nameof(OidcProxyConfig)}.{nameof(config.ReverseProxy)}.{nameof(config.ReverseProxy.Clusters)}.");
        }

        return serviceCollection.AddOidcProxy(options =>
        {
            AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
            AssignIfNotNull(config.LandingPage, options.SetLandingPage);
            AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
            AssignIfNotNull(config.SessionCookieName, cookieName => options.SessionCookieName = cookieName);
            
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