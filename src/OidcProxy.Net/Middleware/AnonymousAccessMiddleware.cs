using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Middleware;

internal class AnonymousAccessMiddleware : IMiddleware
{
    private readonly EndpointName _endpointName;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProxyOptions _options;
    private readonly AuthSession _authSession;
    private readonly ILogger _logger;
    private readonly IRedirectUriFactory _redirectUriFactory;
    private readonly IIdentityProvider _identityProvider;
    
    public AnonymousAccessMiddleware(EndpointName endpointName,
        ProxyOptions options, 
        AuthSession authSession,
        ILogger logger,
        IRedirectUriFactory redirectUriFactory,
        IIdentityProvider identityProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _endpointName = endpointName;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _authSession = authSession;
        _logger = logger;
        _redirectUriFactory = redirectUriFactory;
        _identityProvider = identityProvider;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (_options.AllowAnonymousAccess)
        {
            await next(context);
            return;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session?.GetAccessToken() != null)
        {
            await next(context);
        }

        var redirectUri = _redirectUriFactory.DetermineRedirectUri(context, _endpointName.ToString());
        
        var authorizeRequest = await _identityProvider.GetAuthorizeUrlAsync(redirectUri);
        
        if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
        {
            await _authSession.SetCodeVerifierAsync(authorizeRequest.CodeVerifier);
        }
        
        await _logger.InformAsync($"Redirect({authorizeRequest.AuthorizeUri})");
        
        context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
    }

}