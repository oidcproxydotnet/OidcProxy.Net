using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication;

public interface IAuthenticationCallbackHandler
{
    Task<IResult> OnAuthenticationFailed(HttpContext context);
    Task<IResult> OnAuthenticated(HttpContext context);
    Task OnError(HttpContext context, Exception e);
}