using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Endpoints;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers;

internal class IdpRegistration<TIdentityProvider, TOptions> : IIdpRegistration where TIdentityProvider : class, IIdentityProvider
    where TOptions : class
{
    private readonly Action<IServiceCollection> _idpRegistration;

    private readonly Action<WebApplication> _idpEndpointRegistration;

    private readonly Action<IReverseProxyBuilder> _proxyConfiguration; // done

    private readonly List<Type> _yarpMiddlewareRegistrations = [typeof(TokenRenewalMiddleware)]; // done

    public IdpRegistration(TOptions options, string endpointName = ".auth")
    {
        _idpRegistration = serviceCollection =>
        {
            serviceCollection
                .AddSingleton<EndpointName>(_ => new EndpointName(endpointName))
                .AddTransient<TokenRenewalMiddleware>()
                .AddTransient<IIdentityProvider, TIdentityProvider>()
                .AddTransient(_ => options)
                .AddHttpClient<TIdentityProvider>(); // all done
        };

        _proxyConfiguration = c =>
        {
            c.AddTransforms<HttpHeaderTransformation>(); // done
        };

        _idpEndpointRegistration = app => app.MapAuthenticationEndpoints(endpointName); // done
    }

    public void AddYarpMiddleware(Type handlerType)
    {
        _yarpMiddlewareRegistrations.Add(handlerType); // done
    }

    public void Apply(IServiceCollection serviceCollection)
    {
        _idpRegistration.Invoke(serviceCollection); // done 
    }

    public void Apply(WebApplication app)
    {
        _idpEndpointRegistration.Invoke(app);
        app.RegisterYarpMiddleware(_yarpMiddlewareRegistrations); // done
    }

    public void Apply(IReverseProxyBuilder configuration)
    {
        _proxyConfiguration.Invoke(configuration);  // done
    }
}