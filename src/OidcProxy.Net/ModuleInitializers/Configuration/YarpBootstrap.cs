using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal class YarpBootstrap : IBootstrap
{
    private readonly List<Type> _yarpMiddlewareRegistrations = [typeof(TokenRenewalMiddleware)];

    private Action<IReverseProxyBuilder> _configuration = _ => { };

    public void AddYarpMiddleware(IEnumerable<Type> handlerType)
    {
        _yarpMiddlewareRegistrations.AddRange(handlerType);
    }
    
    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        var proxyBuilder = services
            .AddReverseProxy()
            .AddTransforms<HttpHeaderTransformation>();
        
        _configuration.Invoke(proxyBuilder);
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {
        app.RegisterYarpMiddleware(_yarpMiddlewareRegistrations);
    }

    public void WithPostConfigure(Action<IReverseProxyBuilder> configuration)
    {
        _configuration = configuration;
    }
}