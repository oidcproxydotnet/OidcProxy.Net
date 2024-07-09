using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Locking;
using OidcProxy.Net.Locking.Distributed.Redis;
using OidcProxy.Net.Locking.InMemory;
using OidcProxy.Net.Logging;
using OidcProxy.Net.Middleware;
using OidcProxy.Net.OpenIdConnect;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace OidcProxy.Net.ModuleInitializers;

public class ProxyOptions
{
    internal IIdpRegistration? IdpRegistration = null;

    private Action<IReverseProxyBuilder> _applyReverseProxyConfiguration = _ => { };

    private Action<IServiceCollection> _applyClaimsTransformationRegistration = (s) => s.AddTransient<IClaimsTransformation, DefaultClaimsTransformation>();

    private Action<IServiceCollection> _applyBackBone = (s) => s.AddTransient<IConcurrentContext, InMemoryConcurrentContext>();

    private Action<IServiceCollection> _applyAuthenticationCallbackHandlerRegistration = (s) => s.AddTransient<IAuthenticationCallbackHandler, DefaultAuthenticationCallbackHandler>();

    private Action<IServiceCollection> _applyJwtParser = (s) => s.AddTransient<ITokenParser, JwtParser>().AddSingleton<IEncryptionKey?>(_ => null);

    private Action<IServiceCollection> _applyJwtValidator = (s) => s.AddTransient<IJwtSignatureValidator, JwtSignatureValidator>();
    
    private Action<IServiceCollection> _applyHs256SignatureValidator = (s) => s.AddTransient<Hs256SignatureValidator>(_ => null);

    private List<Type> _customYarpMiddleware = [];

    internal Uri? CustomHostName = null;

    internal LandingPage ErrorPage;

    internal LandingPage LandingPage;

    internal LandingPage[] AllowedUserPreferredLandingPages = Array.Empty<LandingPage>();

    /// <summary>
    /// Gets or sets the name of the cookie.
    /// </summary>
    public string CookieName { get; set; } = "oidcproxy.cookie";

    /// <summary>
    /// Get or set a value that indicates the amount of time of inactivity after which the session will be abandoned.
    /// </summary>
    public TimeSpan SessionIdleTimeout { get; set; } = TimeSpan.FromMinutes(20);

    /// <summary>
    /// Gets ors sets a value which indicates whether or not the redirect_uri will automatically be rewritten to http
    /// instead of https. This feature might come in handy when hosting the software in a Docker image.
    /// </summary>
    public bool AlwaysRedirectToHttps { get; set; } = true;

    /// <summary>
    /// Gets or sets a value that indicates whether or not the user is allowed to specify the page he/she wants to be
    /// redirected to after authenticating successfully.
    /// </summary>
    public bool EnableUserPreferredLandingPages { get; set; } = false;

    /// <summary>
    /// The name of the claim that represents the username.
    /// </summary>
    public string NameClaim { get; set; } = "sub";

    /// <summary>
    /// The name of the claim that represents the username.
    /// </summary>
    public string RoleClaim { get; set; } = "role";

    /// <summary>
    /// Allow or disallow anonymous access. When set to false, unauthenticated requests are redirected to the
    /// /authorize endpoint of the identity provider.
    /// </summary>
    public bool AllowAnonymousAccess { get; set; } = true;

    /// <summary>
    /// Sets a custom page to redirect to when the authentication on the OIDC Server failed.
    /// The url will be augmented with an additional query string parameter to indicate what error occured.
    /// </summary>
    /// <param name="errorPage">A relative path to the error page</param>
    public void SetAuthenticationErrorPage(string errorPage)
    {
        const string errorMessage = "GNC-B-faa80ff1e452: " +
                                    "Cannot initialize OidcProxy.Net. " +
                                    "Invalid error page. " +
                                    "The path to the error page must be relative and may not have a querystring.";

        if (!LandingPage.TryParse(errorPage, out var value))
        {
            throw new NotSupportedException(errorMessage);
        }

        if (errorPage.Contains('?') || errorPage.Contains('#'))
        {
            throw new NotSupportedException(errorMessage);
        }

        ErrorPage = value;
    }

    /// <summary>
    /// Set the page the user will be redirected to after authenticating successfully
    /// </summary>
    /// <param name="landingPage">The relative path the user will be redirected to.</param>
    public void SetLandingPage(string landingPage)
    {
        if (!LandingPage.TryParse(landingPage, out var value))
        {
            const string errorMessage = "GNC-B-f30ab76dde63: " +
                                        "Cannot initialize OidcProxy.Net. " +
                                        "Invalid landing page. " +
                                        "The path to the landing page must be relative.";

            throw new NotSupportedException(errorMessage);
        }

        LandingPage = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="landingPage"></param>
    public void SetAllowedLandingPages(IEnumerable<string>? landingPages)
    {
        if (landingPages == null || !landingPages.Any())
        {
            return;
        }

        var allowedLandingPages = new List<LandingPage>();

        foreach (var landingPage in landingPages)
        {
            if (!LandingPage.TryParse(landingPage, out var value))
            {
                const string errorMessage = "GNC-B-f30ab76dde63: " +
                                            "Cannot initialize OidcProxy.Net. " +
                                            "Invalid landing page. " +
                                            "The path to the landing page must be relative.";

                throw new NotSupportedException(errorMessage);
            }

            allowedLandingPages.Add(value);
        }

        AllowedUserPreferredLandingPages = allowedLandingPages.ToArray();
    }

    /// <summary>
    /// The OidcProxy typically derives the redirect URL from the request context as a default behavior. However, in cases where the hosting of an image with the OidcProxy involves proxies or different configurations, the automatically inferred redirect URL may be incorrect. To address this issue, you can utilize the following method to override the default value of the redirect URL.
    /// </summary>
    /// <param name="hostname"></param>
    /// <exception cref="NotSupportedException"></exception>
    public void SetCustomHostName(Uri hostname)
    {
        if (!string.IsNullOrEmpty(hostname.Query))
        {
            throw new NotSupportedException("GCN-B-322cf6ab8a70: " +
                                            "Cannot initialize OidcProxy.Net. " +
                                            "Error configuring custom hostname. " +
                                            $"{hostname} is not a valid hostname. " +
                                            "A custom hostname may not have a querystring.");
        }

        CustomHostName = hostname;
    }

    public void RegisterIdentityProvider<TIdentityProvider, TOptions>(TOptions options, string endpointName = ".auth")
        where TIdentityProvider : class, IIdentityProvider
        where TOptions : class
    {
        if (IdpRegistration != null)
        {
            throw new NotSupportedException("Unable to bootstrap OidcProxy.Net. " +
                                            "Configuring multiple IdentityProviders is not supported.");
        }

        var registration = new IdpRegistration<TIdentityProvider, TOptions>(options, endpointName);

        foreach (var handlerType in _customYarpMiddleware)
        {
            registration.AddYarpMiddleware(handlerType);
        }

        IdpRegistration = registration;
    }

    /// <summary>
    /// Adds middleware into the YARP processing pipeline.
    /// </summary>
    /// <typeparam name="THandler">The type implementing the middleware.</typeparam>
    public void AddYarpMiddleware<THandler>() where THandler : IYarpMiddleware
    {
        _customYarpMiddleware.Add(typeof(THandler));
    }

    /// <summary>
    /// By default, the /{0}/me endpoint displays the payload of the ID token, including all the claims. However, there may be situations where it is necessary to display fewer claims or additional claims are required. To customize the claims shown in the /{0}/me endpoint, you can create a custom implementation of the IClaimsTransformation interface and register it using this method. This allows you to control the transformation and selection of claims for the endpoint. 
    /// </summary>
    /// <typeparam name="TClaimsTransformation">The class that augments the output of the /{0}/me endpoint</typeparam>
    public void AddClaimsTransformation<TClaimsTransformation>() where TClaimsTransformation : class, IClaimsTransformation
    {
        _applyClaimsTransformationRegistration = s => s.AddTransient<IClaimsTransformation, TClaimsTransformation>();
    }

    /// <summary>
    /// Configure a class that converts the token received from the identity-provider to an instance of JwtPayload.
    /// </summary>
    /// <typeparam name="TTokenParser">The type of the class to use to convert the jwt to a JwtPayload.</typeparam>
    public void AddTokenParser<TTokenParser>() where TTokenParser : class, ITokenParser
    {
        _applyJwtParser = s => s
            .AddTransient<ITokenParser, TTokenParser>();
    }

    /// <summary>
    /// Provide the encryption key that is used to decrypt the JWE
    /// </summary>
    /// <param name="key">An implementation of the ITokenEncryptionKey class.</param>
    public void UseEncryptionKey(IEncryptionKey key)
    {
        _applyJwtParser = s => s
            .AddTransient<ITokenParser, JweParser>()
            .AddSingleton(key);
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void UseSigningKey(SymmetricKey key)
    {
        _applyHs256SignatureValidator = s => s.AddTransient<Hs256SignatureValidator>(_ => new Hs256SignatureValidator(key));
    }

    /// <summary>
    /// Configure a class that redirects the user somewhere after authenticating.
    /// </summary>
    /// <typeparam name="TAuthenticationCallbackHandler">The type of the class.</typeparam>
    public void AddAuthenticationCallbackHandler<TAuthenticationCallbackHandler>() where TAuthenticationCallbackHandler : class, IAuthenticationCallbackHandler
    {
        _applyAuthenticationCallbackHandlerRegistration = s => s.AddTransient<IAuthenticationCallbackHandler, TAuthenticationCallbackHandler>();
    }

    /// <summary>
    /// YARP is initially set up to forward traffic based on the predefined configuration. However, if you require additional configuration options, you can utilize this method to extend the configuration.
    /// </summary>
    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        _applyReverseProxyConfiguration = configuration;
    }
    
    /// <summary>
    /// Register a class that validates the access token signature
    /// </summary>
    /// <typeparam name="T">The type to register</typeparam>
    /// <exception cref="NotImplementedException"></exception>
    public void RegisterSignatureValidator<T>() where T : class, IJwtSignatureValidator
    {
        _applyJwtValidator = s => s.AddTransient<IJwtSignatureValidator, T>();
    }

    /// <summary>
    /// Configure a Redis backbone. This is required to run this module in distributed mode.
    /// </summary>
    /// <param name="connectionMultiplexer"></param>
    /// <param name="httpSessionKey"></param>
    /// <param name="redisInstanceName"></param>
    public void ConfigureRedisBackBone(ConnectionMultiplexer connectionMultiplexer)
    {
        _applyBackBone = (serviceCollection) =>
        {
            serviceCollection
                .AddDataProtection()
                .PersistKeysToStackExchangeRedis(connectionMultiplexer, this.CookieName);

            serviceCollection.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = connectionMultiplexer.Configuration;
                options.InstanceName = CookieName;
            });

            serviceCollection
                .AddTransient<IConcurrentContext, RedisConcurrentContext>()
                .AddTransient<IDistributedLockFactory>(_ => RedLockFactory.Create(new List<RedLockMultiplexer>() { connectionMultiplexer }));
        };
    }

    /// <summary>
    /// Apply the options to the service collection
    /// </summary>
    public void Apply(IServiceCollection serviceCollection)
    {
        if (IdpRegistration == null)
        {
            throw new NotSupportedException("Unable to bootstrap OidcProxy.Net. " +
                                            "You must register an IdentityProvider.");
        }

        var proxyBuilder = serviceCollection
            .AddReverseProxy();

        _applyReverseProxyConfiguration(proxyBuilder);

        _applyBackBone(serviceCollection);

        IdpRegistration.Apply(proxyBuilder);

        IdpRegistration.Apply(serviceCollection);

        _applyClaimsTransformationRegistration(serviceCollection);
        _applyAuthenticationCallbackHandlerRegistration(serviceCollection);
        _applyJwtParser(serviceCollection);
        _applyJwtValidator(serviceCollection);
        _applyHs256SignatureValidator(serviceCollection);

        serviceCollection
            .AddDistributedMemoryCache()
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = SessionIdleTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = CookieName;
            });

        serviceCollection
            .AddTransient(_ => this)
            .AddTransient<IRedirectUriFactory, RedirectUriFactory>();

        serviceCollection
            .AddHttpContextAccessor()
            .AddTransient<TokenFactory>()
            .AddTransient<AuthSession>()
            .AddTransient<IAuthSession, AuthSession>()
            .AddTransient<ILogger, DefaultLogger>();

        serviceCollection
            .AddTransient<Rs256SignatureValidator>();

        serviceCollection
            .AddAuthentication(OidcProxyAuthenticationHandler.SchemaName)
            .AddScheme<OidcProxyAuthenticationSchemeOptions, OidcProxyAuthenticationHandler>(OidcProxyAuthenticationHandler.SchemaName, null);
    }
}