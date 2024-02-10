using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OpenIddict.Validation;

namespace OidcProxy.Net.OpenIdDict;

public static class ModuleInitializer
{
    public static OidcProxyConfigurationBuilder AddOpenIdDictProxy(this IServiceCollection serviceCollection,
        OpenIdDictProxyConfig config,
        Action<ProxyOptions>? configureOptions = null)
    {
        serviceCollection.AddOidcProxy(config, o =>
        {
            configureOptions?.Invoke(o);
        });

        return new OidcProxyConfigurationBuilder(serviceCollection, config);
    }

    public static WebApplication UseOpenIdDictProxy(this WebApplication app) => app.UseOidcProxy();
}