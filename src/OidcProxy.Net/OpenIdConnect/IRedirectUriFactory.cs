using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.OpenIdConnect;

internal interface IRedirectUriFactory
{
    string DetermineHostName(HttpContext context);
    string DetermineRedirectUri(HttpContext context, PathString endpointName);
}