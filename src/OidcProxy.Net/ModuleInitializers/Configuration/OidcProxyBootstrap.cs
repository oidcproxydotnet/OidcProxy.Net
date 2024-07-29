using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Endpoints;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Logging;
using OidcProxy.Net.Middleware;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal class OidcProxyBootstrap<TIdentityProvider, TIdentityProviderConfig>(TIdentityProviderConfig config) 
    : IOidcProxyBootstrap 
    where TIdentityProvider : class, IIdentityProvider
    where TIdentityProviderConfig : class
{
    private Type _callbackHandlerType = typeof(DefaultAuthenticationCallbackHandler);

    private Type _claimsTransformationType = typeof(DefaultClaimsTransformation);
    
    public IOidcProxyBootstrap WithCallbackHandler<TCallbackHandler>()
        where TCallbackHandler : IAuthenticationCallbackHandler
    {
        _callbackHandlerType = typeof(TCallbackHandler);
        return this;
    }

    public IOidcProxyBootstrap WithClaimsTransformation<TClaimsTransformation>()
        where TClaimsTransformation : IClaimsTransformation
    {
        _claimsTransformationType = typeof(TClaimsTransformation);
        return this;
    }

    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services
            .AddTransient<AnonymousAccessMiddleware>();
        
        services
            .AddSingleton<EndpointName>(_ => new EndpointName(options.EndpointName))
            .AddTransient<TokenRenewalMiddleware>()
            .AddTransient<IIdentityProvider, TIdentityProvider>()
            .AddTransient<TIdentityProviderConfig>(_ => config)
            .AddTransient(_ => options)
            .AddHttpClient<TIdentityProvider>();

        services
            .AddTransient<IRedirectUriFactory, RedirectUriFactory>();
        
        services
            .AddHttpContextAccessor()
            .AddTransient<TokenFactory>()
            .AddTransient<AuthSession>()
            .AddTransient<IAuthSession, AuthSession>()
            .AddTransient<ILogger, DefaultLogger>();
        
        services
            .AddTransient<Rs256SignatureValidator>();

        services
            .AddTransient(typeof(IAuthenticationCallbackHandler), _callbackHandlerType)
            .AddTransient(typeof(IClaimsTransformation), _claimsTransformationType);
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {
        if (!options.AllowAnonymousAccess)
        {
            app.UseMiddleware<AnonymousAccessMiddleware>();
        }
        
        app.MapAuthenticationEndpoints(options.EndpointName);
    }
}