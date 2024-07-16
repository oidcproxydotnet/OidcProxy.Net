using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Endpoints;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Logging;
using OidcProxy.Net.Middleware;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal class OidcProxyBootstrap<TIdentityProvider>(string endpointName) : IBootstrap
    where TIdentityProvider : class, IIdentityProvider
{
    private Type _callbackHandlerType = typeof(DefaultAuthenticationCallbackHandler);

    private Type _claimsTransformationType = typeof(DefaultClaimsTransformation);
    
    public OidcProxyBootstrap<TIdentityProvider> WithCallbackHandler<TCallbackHandler>()
        where TCallbackHandler : IAuthenticationCallbackHandler
    {
        _callbackHandlerType = typeof(TCallbackHandler);
        return this;
    }

    public OidcProxyBootstrap<TIdentityProvider> WithClaimsTransformation<TClaimsTransformation>()
        where TClaimsTransformation : IClaimsTransformation
    {
        _claimsTransformationType = typeof(TClaimsTransformation);
        return this;
    }

    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services
            .AddSingleton<EndpointName>(_ => new EndpointName(endpointName))
            .AddTransient<TokenRenewalMiddleware>()
            .AddTransient<IIdentityProvider, TIdentityProvider>()
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
        app.MapAuthenticationEndpoints(endpointName);
    }
}