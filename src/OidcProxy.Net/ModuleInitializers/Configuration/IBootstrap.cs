using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal interface IBootstrap
{
    void Configure(ProxyOptions options, IServiceCollection services);

    void Configure(ProxyOptions options, WebApplication app);
}