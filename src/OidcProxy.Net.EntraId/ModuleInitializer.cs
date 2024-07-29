using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.EntraId;

public static class ModuleInitializer
{
    public static void ConfigureEntraId(this ProxyOptions options, IConfigurationSection configurationSection,
        string endpointName = ".auth")
        => ConfigureEntraId(options, (IConfigurationSection)configurationSection.Get<EntraIdConfig>(), endpointName);

    public static void ConfigureEntraId(this ProxyOptions options, EntraIdConfig config, string endpointName = ".auth")
    {
        if (!config.Validate(out var errors))
        {
            throw new NotSupportedException(string.Join(", ", errors));
        }

        options.RegisterIdentityProvider<EntraIdIdentityProvider, EntraIdConfig>(config, endpointName);
        options.RegisterSignatureValidator<EntraIdJwtSignatureValidator>();
    }

    /// <summary>
    /// Initialises the BFF. Also use app.UseOidcProxy();
    /// </summary>
    /// <param name="serviceCollection"></param>
    /// <param name="config"></param>
    /// <returns></returns>
    public static IServiceCollection AddEntraIdProxy(this IServiceCollection serviceCollection,
        EntraIdProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        if (config == null)
        {
            throw new ArgumentNullException(nameof(config),
                "Failed to initialise OidcProxy.Net. Config cannot be null. " +
                $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}`.");
        }

        var entraIdConfig = config.EntraId;
        var endpointName = config.EndpointName ?? ".auth";
        var routes = config.ReverseProxy?.Routes.ToRouteConfig();
        var clusters = config.ReverseProxy?.Clusters.ToClusterConfig();

        if (entraIdConfig == null)
        {
            throw new ArgumentException("Failed to initialise OidcProxy.Net. " +
                                        $"Invoke `builder.Services.AddOidcProxy(..)` with an instance of `{nameof(EntraIdProxyConfig)}` " +
                                        $"and provide a value for {nameof(EntraIdProxyConfig)}.{nameof(config.EntraId)}.");
        }

        return serviceCollection.AddOidcProxy(options =>
        {
            AssignIfNotNull(config.ErrorPage, options.SetAuthenticationErrorPage);
            AssignIfNotNull(config.LandingPage, options.SetLandingPage);
            AssignIfNotNull(config.CustomHostName, options.SetCustomHostName);
            AssignIfNotNull(config.CookieName, cookieName => options.CookieName = cookieName);
            AssignIfNotNull(config.NameClaim, nameClaim => options.NameClaim = nameClaim);
            AssignIfNotNull(config.RoleClaim, roleClaim => options.RoleClaim = roleClaim);

            options.Mode = config.Mode;
            options.EnableUserPreferredLandingPages = config.EnableUserPreferredLandingPages;
            options.AlwaysRedirectToHttps =
                !config.AlwaysRedirectToHttps.HasValue || config.AlwaysRedirectToHttps.Value;
            options.AllowAnonymousAccess = !config.AllowAnonymousAccess.HasValue || config.AllowAnonymousAccess.Value;
            options.SetAllowedLandingPages(config.AllowedLandingPages);

            if (config.SessionIdleTimeout.HasValue)
            {
                options.SessionIdleTimeout = config.SessionIdleTimeout.Value;
            }

            configureOptions?.Invoke(options);

            ConfigureEntraId(options, entraIdConfig, endpointName);

            if (options.Mode != Mode.AuthenticateOnly)
            {
                options.ConfigureYarp(routes, clusters);
            }
        });
    }

    public static void UseEntraIdProxy(this WebApplication app) => app.UseOidcProxy();

    private static void AssignIfNotNull<T>(T? value, Action<T> @do)
    {
        if (value != null)
        {
            @do(value);
        }
    }
}