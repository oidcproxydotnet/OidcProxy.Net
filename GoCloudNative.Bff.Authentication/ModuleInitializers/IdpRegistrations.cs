using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class IdpRegistrations
{
    private List<string> _endpointNames = new();
    
    private List<Type> _endpointTypes = new();
    
    private readonly List<Action<IServiceCollection>> _idpRegistrations = new();
    
    private readonly List<Action<WebApplication>> _idpEndpointRegistrations = new();

    private readonly List<Action<IReverseProxyBuilder>> _proxyConfigurations = new();

    public void Register<TIdentityProvider, TOptions>(TOptions options, string endpointName = "account")
        where TIdentityProvider : class, IIdentityProvider
        where TOptions : class
    {
        AssertEndpointNameNotRegisteredTwice(endpointName);

        AssertIdentityProviderTypeNotRegisteredTwice<TIdentityProvider>();

        _idpRegistrations.Add(s => s
            .AddTransient<TIdentityProvider>()
            .AddSingleton(_ => options)
            .AddHttpClient<TIdentityProvider>()
        );

        _proxyConfigurations.Add(c => { c.AddTransforms<HttpHeaderTransformation<TIdentityProvider>>(); });

        _idpEndpointRegistrations.Add(app => app.MapAuthenticationEndpoints<TIdentityProvider>(endpointName));
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
    }

    public void Apply(IReverseProxyBuilder configuration)
    {
        foreach (var proxyConfiguration in _proxyConfigurations)
        {
            proxyConfiguration.Invoke(configuration);
        }
    }
    
    private void AssertEndpointNameNotRegisteredTwice(string endpointName)
    {
        if (_endpointNames.Any(x => x.Equals(endpointName, StringComparison.InvariantCultureIgnoreCase)))
        {
            throw new NotSupportedException("Failed to start GoCloudNative.BFF. " +
                                            "Registering multiple TIdentityProvider types on the same endpoint is not supported." +
                                            "Remove one of the IdentityProviderConfigurations or configure another endpointName.");
        }

        _endpointNames.Add(endpointName);
    }

    private void AssertIdentityProviderTypeNotRegisteredTwice<TIdentityProvider>()
    {
        if (_endpointTypes.Contains(typeof(TIdentityProvider)))
        {
            throw new NotSupportedException("Failed to start GoCloudNative.BFF. " +
                                            "Registering multiple TIdentityProvider types on the same endpoint is not supported." +
                                            "Remove one of the IdentityProviderConfigurations or configure another endpointName.");
        }

        _endpointTypes.Add(typeof(TIdentityProvider));
    }
}