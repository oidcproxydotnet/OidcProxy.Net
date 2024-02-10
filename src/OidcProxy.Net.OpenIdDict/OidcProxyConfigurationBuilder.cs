using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.OpenIdDict;

public class OidcProxyConfigurationBuilder
{
    private readonly IServiceCollection _serviceCollection;
    private readonly OpenIdDictProxyConfig _config;

    public OidcProxyConfigurationBuilder(IServiceCollection serviceCollection, 
        OpenIdDictProxyConfig config)
    {
        _serviceCollection = serviceCollection;
        _config = config;
    }

    public void AddValidation(Action<OpenIddictValidationBuilder> @delegate)
    {
        _serviceCollection
            .AddOpenIddict()
            .AddValidation(x =>
            {
                @delegate(x);

                x.SetIssuer(_config.Oidc.Authority);
                x.UseSystemNetHttp();
            });
        
        _serviceCollection.AddTransient<ITokenParser, OpenIdDictJwtParser>();
    }
}