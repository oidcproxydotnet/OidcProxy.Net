using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace OidcProxy.Net.ModuleInitializers;

internal interface IIdpRegistration
{
    void Apply(IServiceCollection serviceCollection);
    void Apply(WebApplication app);
    void Apply(IReverseProxyBuilder configuration);
}