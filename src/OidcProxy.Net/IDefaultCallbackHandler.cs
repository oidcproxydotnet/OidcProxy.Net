using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net;

public interface IAuthenticationCallbackHandler
{
    Task<IResult> OnAuthenticationFailed(HttpContext context, 
        string defaultLandingPage, 
        string? userPreferredLandingPage);
    Task<IResult> OnAuthenticated(HttpContext context, 
        string defaultLandingPage, 
        string? userPreferredLandingPage);
    
    Task OnError(HttpContext context, Exception e);
}