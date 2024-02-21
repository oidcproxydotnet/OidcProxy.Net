using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.Logging;
using ILogger = OidcProxy.Net.Logging.ILogger;

namespace OidcProxy.Net;

public class DefaultAuthenticationCallbackHandler : IAuthenticationCallbackHandler
{
    private readonly ILogger _logger;

    public DefaultAuthenticationCallbackHandler(ILogger logger)
    {
        _logger = logger;
    }

    public virtual async Task<IResult> OnAuthenticationFailed(HttpContext context, 
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        // Todo: Introduce proper error page here
        var landingPage = userPreferredLandingPage ?? defaultLandingPage;
        
        await _logger.InformAsync($"Redirect({landingPage})");
        return Results.Redirect(landingPage);
    }

    public virtual async Task<IResult> OnAuthenticated(HttpContext context, 
        JwtPayload? jwtPayload,
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        var landingPage = userPreferredLandingPage ?? defaultLandingPage;
        
        await _logger.InformAsync($"Redirect({landingPage})");
        return Results.Redirect(landingPage);
    }

    public virtual Task OnError(HttpContext context, Exception e)
    {
        return Task.CompletedTask;
    }
}