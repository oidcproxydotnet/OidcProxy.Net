using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Logging;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Middleware;

internal class AnonymousAccessMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ProxyOptions _options;
    private readonly AuthSession _authSession;
    private readonly ILogger _logger;
    private readonly IRedirectUriFactory _redirectUriFactory;
    private readonly IIdentityProvider _identityProvider;

    public AnonymousAccessMiddleware(RequestDelegate next,
        ProxyOptions options, 
        AuthSession authSession,
        ILogger logger,
        IRedirectUriFactory redirectUriFactory,
        IIdentityProvider identityProvider,
        IHttpContextAccessor httpContextAccessor)
    {
        _next = next;
        _httpContextAccessor = httpContextAccessor;
        _options = options;
        _authSession = authSession;
        _logger = logger;
        _redirectUriFactory = redirectUriFactory;
        _identityProvider = identityProvider;
    }

    public async Task Invoke(HttpContext context)
    {
        if (_options.AllowAnonymousAccess)
        {
            await _next(context);
            return;
        }

        var session = _httpContextAccessor.HttpContext?.Session;
        if (session?.GetAccessToken() != null)
        {
            await _next(context);
        }

        await _logger.InformAsync("Todo: Redirect to /authorize endpoint");
        await _next(context);
        return;
        
        // Todo: Implement obtaining the correct /.auth endpoint because it is a variable
        
        var redirectUri = _redirectUriFactory.DetermineRedirectUri(context, "/.auth");
        
        var authorizeRequest = await _identityProvider.GetAuthorizeUrlAsync(redirectUri);
        
        if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
        {
            await _authSession.SetCodeVerifierAsync(authorizeRequest.CodeVerifier);
        }
        
        await _logger.InformAsync($"Redirect({authorizeRequest.AuthorizeUri})");
        
        context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
    }
}