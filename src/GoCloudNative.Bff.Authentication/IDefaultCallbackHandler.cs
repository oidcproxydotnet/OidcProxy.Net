using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public interface IAuthenticationCallbackHandler
{
    Task<IResult> OnAuthenticationFailed(HttpContext context, string defaultRedirectUrl);
    Task<IResult> OnAuthenticated(HttpContext context, string defaultRedirectUrl);
    Task OnError(HttpContext context, Exception e);
}