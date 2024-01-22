using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net;

public class DefaultAuthenticationCallbackHandler : IAuthenticationCallbackHandler
{
    private readonly ILogger<DefaultAuthenticationCallbackHandler> _logger;

    public DefaultAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger)
    {
        _logger = logger;
    }

    public virtual Task<IResult> OnAuthenticationFailed(HttpContext context, 
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        // Todo: Introduce proper error page here
        var landingPage = userPreferredLandingPage ?? defaultLandingPage;
        
        _logger.LogLine(context, $"Redirect({landingPage})");
        var redirectResult = Results.Redirect(landingPage);

        return Task.FromResult(redirectResult);
    }

    public virtual Task<IResult> OnAuthenticated(HttpContext context, 
        JwtPayload? jwtPayload,
        string defaultLandingPage, 
        string? userPreferredLandingPage)
    {
        var landingPage = userPreferredLandingPage ?? defaultLandingPage;
        
        _logger.LogLine(context, $"Redirect({landingPage})");
        var redirectResult = Results.Redirect(landingPage);

        return Task.FromResult(redirectResult);
    }

    public virtual Task OnError(HttpContext context, Exception e)
    {
        return Task.CompletedTask;
    }
}