using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Middleware;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal class AuthorizationBootstrap : IBootstrap
{
    private Action<IServiceCollection> _applyJwtParser = s => s
        .AddTransient<ITokenParser, JwtParser>()
        .AddSingleton<IEncryptionKey?>(_ => null);
    
    private Action<IServiceCollection> _applyJwtValidator = s => s
        .AddTransient<IJwtSignatureValidator, JwtSignatureValidator>();
    
    private Action<IServiceCollection> _applyHs256SignatureValidator = s => s
        .AddTransient<Hs256SignatureValidator>(_ => null);
    
    public AuthorizationBootstrap WithTokenParser<TTokenParser>() where TTokenParser : class, ITokenParser
    {
        _applyJwtParser = s => s
            .AddTransient<ITokenParser, TTokenParser>();

        return this;
    }

    public AuthorizationBootstrap WithEncryptionKey(IEncryptionKey key)
    {
        _applyJwtParser = s => s
            .AddTransient<ITokenParser, JweParser>()
            .AddSingleton(key);

        return this;
    }
    
    public AuthorizationBootstrap WithSigningKey(SymmetricKey key)
    {
        _applyHs256SignatureValidator = s => s
            .AddTransient<Hs256SignatureValidator>(_ => new Hs256SignatureValidator(key));

        return this;
    }
    
    public AuthorizationBootstrap WithSignatureValidator<T>() where T : class, IJwtSignatureValidator
    {
        _applyJwtValidator = s => s.AddTransient<IJwtSignatureValidator, T>();

        return this;
    }

    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services
            .AddAuthentication(OidcProxyAuthenticationHandler.SchemaName)
            .AddScheme<OidcProxyAuthenticationSchemeOptions, OidcProxyAuthenticationHandler>(OidcProxyAuthenticationHandler.SchemaName, null);
        
        _applyJwtParser(services); 
        _applyJwtValidator(services);
        _applyHs256SignatureValidator(services);
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {        
        app.UseAuthentication();
        app.UseAuthorization();
    }
}