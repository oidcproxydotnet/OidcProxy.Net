using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.OpenIdConnect;

public interface IRedirectUriFactory
{
    string DetermineHostName(HttpContext context);
    string DetermineRedirectUri(HttpContext context, PathString endpointName);
}