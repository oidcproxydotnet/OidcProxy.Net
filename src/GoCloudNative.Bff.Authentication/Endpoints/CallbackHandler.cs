using GoCloudNative.Bff.Authentication.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal class DefaultAuthenticationCallbackHandler : IAuthenticationCallbackHandler
{
    private readonly ILogger<DefaultAuthenticationCallbackHandler> _logger;

    public DefaultAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger)
    {
        _logger = logger;
    }

    public virtual Task<IResult> OnAuthenticationFailed(HttpContext context, string defaultRedirectUrl)
    {
        _logger.LogLine(context, $"Redirect({defaultRedirectUrl})");
        var redirect = Results.Redirect(defaultRedirectUrl);
        
        return Task.FromResult(redirect);
    }

    public virtual Task<IResult> OnAuthenticated(HttpContext context, string defaultRedirectUrl)
    {
        _logger.LogLine(context, $"Redirect({defaultRedirectUrl})");
        var redirectResult = Results.Redirect(defaultRedirectUrl);

        return Task.FromResult(redirectResult);
    }

    public virtual Task OnError(HttpContext context, Exception e)
    {
        return Task.CompletedTask;
    }
}