using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.OpenIdConnect;

public interface IRedirectUriFactory
{
    string DetermineHostName();
    string DetermineRedirectUri(PathString endpointName);
}