using GoCloudNative.Bff.Authentication.Endpoints;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking;
using GoCloudNative.Bff.Authentication.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

internal class IdpRegistrations
{
    private readonly List<string> _endpointNames = new();
    
    private readonly List<Type> _endpointTypes = new();
    
    private readonly List<Type> _optionTypes = new();
    
    private readonly List<Action<IServiceCollection>> _idpRegistrations = new();
    
    private readonly List<Type> _yarpMiddlewareRegistrations = new();
    
    private readonly List<Action<WebApplication>> _idpEndpointRegistrations = new();

    private readonly List<Action<IReverseProxyBuilder>> _proxyConfigurations = new();

    public void Register<TIdentityProvider, TOptions>(TOptions options, string endpointName = "account")
        where TIdentityProvider : class, IIdentityProvider
        where TOptions : class
    {
        AssertEndpointNameRegisteredOnce(endpointName);

        AssertIdentityProviderTypeRegisteredOnce<TIdentityProvider>();

        AssertOptionsTypeRegisteredOnce<TOptions>();

        _idpRegistrations.Add(s => s
            .AddTransient<TokenRenewalMiddleware<TIdentityProvider>>()
            .AddTransient<IConcurrentContext, SingleInstance>()
            .AddTransient<TIdentityProvider>()
            .AddTransient(_ => options)
            .AddHttpClient<TIdentityProvider>()
        );

        _proxyConfigurations.Add(c => { c.AddTransforms<HttpHeaderTransformation<TIdentityProvider>>(); });

        _idpEndpointRegistrations.Add(app => app.MapAuthenticationEndpoints<TIdentityProvider>(endpointName));
        
        _yarpMiddlewareRegistrations.Add(typeof(TokenRenewalMiddleware<TIdentityProvider>));
    }
    
    public void Apply(IServiceCollection serviceCollection)
    {
        foreach (var registration in _idpRegistrations)
        {
            registration.Invoke(serviceCollection);
        }
    }

    public void Apply(WebApplication app)
    {
        foreach (var endpointRegistration in _idpEndpointRegistrations)
        {
            endpointRegistration.Invoke(app);
        }

        app.RegisterYarpMiddleware(_yarpMiddlewareRegistrations);
    }

    public void Apply(IReverseProxyBuilder configuration)
    {
        foreach (var proxyConfiguration in _proxyConfigurations)
        {
            proxyConfiguration.Invoke(configuration);
        }
    }
    
    private void AssertEndpointNameRegisteredOnce(string endpointName)
    {
        if (_endpointNames.Any(x => x.Equals(endpointName, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new NotSupportedException("GCN-B-f204c0800192: " +
                                            "Failed to start GoCloudNative.BFF. " +
                                            "Registering multiple TIdentityProvider types on the same endpoint is not supported." +
                                            "Remove one of the IdentityProviderConfigurations or configure another endpointName.");
        }

        _endpointNames.Add(endpointName);
    }

    private void AssertIdentityProviderTypeRegisteredOnce<TIdentityProvider>()
    {
        if (_endpointTypes.Contains(typeof(TIdentityProvider)))
        {
            throw new NotSupportedException("GCN-B-66b217e55cd6: " +
                                            "Failed to start GoCloudNative.BFF. " +
                                            "Registering multiple identity providers of the same type is not supported.");
        }

        _endpointTypes.Add(typeof(TIdentityProvider));
    }
    
    
    private void AssertOptionsTypeRegisteredOnce<TOptions>()
    {
        if (_optionTypes.Contains(typeof(TOptions)))
        {
            throw new NotSupportedException("GCN-B-7072e626c679: " +
                                            "Failed to start GoCloudNative.BFF. " +
                                            "Registering the same options type is not supported.");
        }

        _optionTypes.Add(typeof(TOptions));
    }
}