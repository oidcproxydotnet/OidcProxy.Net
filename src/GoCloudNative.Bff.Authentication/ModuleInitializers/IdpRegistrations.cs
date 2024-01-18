using GoCloudNative.Bff.Authentication.Endpoints;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking;
using GoCloudNative.Bff.Authentication.Locking.InMemory;
using GoCloudNative.Bff.Authentication.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

internal class IdpRegistration<TIdentityProvider, TOptions> : IIdpRegistration where TIdentityProvider : class, IIdentityProvider
    where TOptions : class
{
    private readonly string _endpointName = "account";

    private readonly Type? _endpointTypes = null;

    private readonly Type? _optionTypes = null;
    
    private Action<IServiceCollection> _idpRegistration = _ => { };
    
    private readonly List<Type> _yarpMiddlewareRegistrations = new();

    private Action<WebApplication> _idpEndpointRegistration = _ => { };

    private Action<IReverseProxyBuilder> _proxyConfiguration = _ => { };

    public IdpRegistration(TOptions options, string endpointName = "account")
    {
        _idpRegistration = serviceCollection =>
        {
            serviceCollection
                .AddTransient<TokenRenewalMiddleware<TIdentityProvider>>()
                .AddTransient<TIdentityProvider>()
                .AddTransient(_ => options)
                .AddHttpClient<TIdentityProvider>();

            serviceCollection
                .TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
                
            serviceCollection
                .AddAuthentication(OidcAuthenticationHandler<TIdentityProvider>.SchemaName)
                .AddScheme<OidcAuthenticationSchemeOptions, OidcAuthenticationHandler<TIdentityProvider>>(OidcAuthenticationHandler<TIdentityProvider>.SchemaName, null);
        };

        _proxyConfiguration = c =>
        {
            c.AddTransforms<HttpHeaderTransformation<TIdentityProvider>>();
        };

        _idpEndpointRegistration = app => app.MapAuthenticationEndpoints<TIdentityProvider>(endpointName);
        
        _yarpMiddlewareRegistrations.Add(typeof(TokenRenewalMiddleware<TIdentityProvider>));
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