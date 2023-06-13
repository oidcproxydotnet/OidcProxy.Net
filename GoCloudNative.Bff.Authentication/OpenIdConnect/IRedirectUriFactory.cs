using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

internal interface IRedirectUriFactory
{
    string DetermineHostName(HttpContext context);
    string DetermineRedirectUri(HttpContext context, string endpointName);
}