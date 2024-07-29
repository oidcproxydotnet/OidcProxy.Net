using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal class AllowAnonymousBootstrap : IBootstrap
{
    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services.AddTransient<AnonymousAccessMiddleware>();
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {        
        if (!options.AllowAnonymousAccess)
        {
            app.UseMiddleware<AnonymousAccessMiddleware>();
        }
    }
}