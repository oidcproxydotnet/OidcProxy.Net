using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

public class YarpBootstrap : IBootstrap
{
    private readonly List<Type> _yarpMiddlewareRegistrations = [typeof(TokenRenewalMiddleware)];
    
    public void AddYarpMiddleware(Type handlerType)
    {
        _yarpMiddlewareRegistrations.Add(handlerType);
    }
    
    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services
            .AddReverseProxy()
            .AddTransforms<HttpHeaderTransformation>();
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {
        app.RegisterYarpMiddleware(_yarpMiddlewareRegistrations);
    }
}