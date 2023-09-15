using GoCloudNative.Bff.Authentication.Logging;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication.Endpoints;

internal class DefaultAuthenticationCallbackHandler : IAuthenticationCallbackHandler
{
    private readonly ILogger<DefaultAuthenticationCallbackHandler> _logger;
    private readonly BffOptions _bffOptions;

    public DefaultAuthenticationCallbackHandler(ILogger<DefaultAuthenticationCallbackHandler> logger, BffOptions bffOptions)
    {
        _logger = logger;
        _bffOptions = bffOptions;
    }

    public Task<IResult> OnAuthenticationFailed(HttpContext context)
    {
        var redirectUri = $"{_bffOptions.ErrorPage}{context.Request.QueryString}";
        _logger.LogLine(context, $"Redirect({redirectUri})");
        var redirect = Results.Redirect(redirectUri);
        
        return Task.FromResult(redirect);
    }

    public Task<IResult> OnAuthenticated(HttpContext context)
    {
        var redirectResult = Results.Redirect(_bffOptions.LandingPage.ToString());

        return Task.FromResult(redirectResult);
    }

    public Task OnError(HttpContext context, Exception e)
    {
        return Task.CompletedTask;
    }
}