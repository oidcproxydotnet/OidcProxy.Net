using OidcProxy.Net.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers;

internal class IdpRegistration<TIdentityProvider, TOptions> : IIdpRegistration where TIdentityProvider : class, IIdentityProvider
    where TOptions : class
{
    private readonly Action<IServiceCollection> _idpRegistration;

    private readonly Action<WebApplication> _idpEndpointRegistration;

    private readonly Action<IReverseProxyBuilder> _proxyConfiguration;
    
    private readonly List<Type> _yarpMiddlewareRegistrations = new();

    public IdpRegistration(TOptions options, string endpointName = ".auth")
    {
        _idpRegistration = serviceCollection =>
        {
            serviceCollection
                .AddTransient<TokenRenewalMiddleware>()
                .AddTransient<IIdentityProvider, TIdentityProvider>()
                .AddTransient(_ => options)
                .AddHttpClient<TIdentityProvider>();
        };

        _proxyConfiguration = c =>
        {
            c.AddTransforms<HttpHeaderTransformation>();
        };

        _idpEndpointRegistration = app => app.MapAuthenticationEndpoints(endpointName);
        
        _yarpMiddlewareRegistrations.Add(typeof(TokenRenewalMiddleware));
    }
    
    public void Apply(IServiceCollection serviceCollection)
    {
        _idpRegistration.Invoke(serviceCollection);
    }

    public void Apply(WebApplication app)
    {
        _idpEndpointRegistration.Invoke(app);
        app.RegisterYarpMiddleware(_yarpMiddlewareRegistrations);
    }

    public void Apply(IReverseProxyBuilder configuration)
    {
        _proxyConfiguration.Invoke(configuration);
    }
}