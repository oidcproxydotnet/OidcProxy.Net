using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Middleware;
using OidcProxy.Net.ModuleInitializers.Configuration;
using OidcProxy.Net.OpenIdConnect;
using StackExchange.Redis;

namespace OidcProxy.Net.ModuleInitializers;

public class ProxyOptions
{
    #region Bootstrapping

    private Action<IOidcProxyBootstrap> _configureCallbackHandler = _ => { };

    private Action<IOidcProxyBootstrap> _configureClaimsTransformation = _ => { };

    private Action<YarpBootstrap> _configureYarpBootstrap = _ => { };

    private readonly List<Type> _customYarpMiddleware = [typeof(TokenRenewalMiddleware)];

    private IOidcProxyBootstrap? _oidcProxyBootstrap = null;

    private readonly SessionBootstrap _sessionBootstrap = new();

    private readonly YarpBootstrap _yarpBootstrap = new();

    private readonly AuthorizationBootstrap _authorizationBootstrap = new();
    
    internal IEnumerable<IBootstrap> GetConfiguration()
    {
        if (_oidcProxyBootstrap == null)
        {
            throw new NotSupportedException("Unable to bootstrap OidcProxy.Net. " +
                                            "No IdentityProviders configured.");
        }
        
        _configureCallbackHandler.Invoke(_oidcProxyBootstrap);
        _configureClaimsTransformation.Invoke(_oidcProxyBootstrap);

        _configureYarpBootstrap.Invoke(_yarpBootstrap);
        _yarpBootstrap.AddYarpMiddleware(_customYarpMiddleware);

        var bootstraps = new IBootstrap?[]
        {
            _yarpBootstrap,
            _sessionBootstrap,
            _oidcProxyBootstrap,
            _authorizationBootstrap,
        };
        
        return bootstraps
            .Where(x => x != null)
            .Cast<IBootstrap>()
            .ToArray();
    }

    #endregion

    #region Public properties

    internal string EndpointName = ".auth";

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

    #endregion

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
    /// Set the whitelist of endpoints a user may return to after authenticating successfully.
    /// </summary>
    /// <param name="landingPages">A collection of endpoints</param>
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

    public void RegisterIdentityProvider<TIdentityProvider, TIdentityProviderConfig>(TIdentityProviderConfig config, string endpointName = ".auth")
        where TIdentityProvider : class, IIdentityProvider
        where TIdentityProviderConfig : class
    {
        if (_oidcProxyBootstrap != null)
        {
            throw new NotSupportedException("Unable to bootstrap OidcProxy.Net. " +
                                            "Configuring multiple IdentityProviders is not supported.");
        }

        EndpointName = endpointName;
        _oidcProxyBootstrap = new OidcProxyBootstrap<TIdentityProvider, TIdentityProviderConfig>(config);
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
    public void AddClaimsTransformation<TClaimsTransformation>()
        where TClaimsTransformation : class, IClaimsTransformation
    {
        _configureClaimsTransformation = oidcProxyBootstrap =>
            oidcProxyBootstrap.WithClaimsTransformation<TClaimsTransformation>();
    }

    /// <summary>
    /// Configure a class that converts the token received from the identity-provider to an instance of JwtPayload.
    /// </summary>
    /// <typeparam name="TTokenParser">The type of the class to use to convert the jwt to a JwtPayload.</typeparam>
    public void AddTokenParser<TTokenParser>() where TTokenParser : class, ITokenParser
    {
        _authorizationBootstrap.WithTokenParser<TTokenParser>();
    }

    /// <summary>
    /// Provide the encryption key that is used to decrypt the JWE
    /// </summary>
    /// <param name="key">An implementation of the ITokenEncryptionKey class.</param>
    public void UseEncryptionKey(IEncryptionKey key)
    {
        _authorizationBootstrap.WithEncryptionKey(key);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public void UseSigningKey(SymmetricKey key)
    {
        _authorizationBootstrap.WithSigningKey(key);
    }

    /// <summary>
    /// Configure a class that redirects the user somewhere after authenticating.
    /// </summary>
    /// <typeparam name="TAuthenticationCallbackHandler">The type of the class.</typeparam>
    public void AddAuthenticationCallbackHandler<TAuthenticationCallbackHandler>()
        where TAuthenticationCallbackHandler : class, IAuthenticationCallbackHandler
    {
        _configureCallbackHandler = oidcProxyBootstrap =>
            oidcProxyBootstrap.WithCallbackHandler<TAuthenticationCallbackHandler>();
    }

    /// <summary>
    /// YARP is initially set up to forward traffic based on the predefined configuration. However, if you require additional configuration options, you can utilize this method to extend the configuration.
    /// </summary>
    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        _configureYarpBootstrap = yarpBootstrap => yarpBootstrap.WithPostConfigure(configuration);
    }

    /// <summary>
    /// Register a class that validates the access token signature
    /// </summary>
    /// <typeparam name="TJwtSignatureValidator">The type to register</typeparam>
    /// <exception cref="NotImplementedException"></exception>
    public void RegisterSignatureValidator<TJwtSignatureValidator>()
        where TJwtSignatureValidator : class, IJwtSignatureValidator
    {
        _authorizationBootstrap.WithSignatureValidator<TJwtSignatureValidator>();
    }

    /// <summary>
    /// Configure a Redis backbone. This is required to run this module in distributed mode.
    /// </summary>
    /// <param name="connectionMultiplexer"></param>
    /// <param name="httpSessionKey"></param>
    /// <param name="redisInstanceName"></param>
    public void ConfigureRedisBackBone(ConnectionMultiplexer connectionMultiplexer)
    {
        _sessionBootstrap.WithRedis(connectionMultiplexer);
    }

    /// <summary>
    /// Apply the options to the service collection
    /// </summary>
    public void Apply(IServiceCollection serviceCollection)
    {
        if (_oidcProxyBootstrap == null)
        {
            throw new NotSupportedException("Unable to bootstrap OidcProxy.Net. " +
                                            "You must register an IdentityProvider.");
        }

        var configuration = GetConfiguration();
        foreach (var bootstrap in configuration)
        {
            bootstrap.Configure(this, serviceCollection);
        }
    }
}