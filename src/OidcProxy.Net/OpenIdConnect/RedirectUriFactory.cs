using Microsoft.AspNetCore.Http;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.OpenIdConnect;

internal class RedirectUriFactory(ProxyOptions options) : IRedirectUriFactory
{
    /// <summary>
    /// Determines the current hostname.
    /// </summary>
    /// <param name="context">The current http context. Needed to determine the current host name.</param>
    /// <returns>The hostname the identity provider should return the auth code to.</returns>
    public string DetermineHostName()
    {
        var hostName = new Uri($"{options.HostName.Scheme}://{options.HostName.Authority}");
        return hostName.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Determines where to redirect to. By default, this software will never redirect to a http address because the redirect url will contain sensitive information.
    /// </summary>
    /// <param name="context">The current http context. Needed to determine the current host name.</param>
    /// <param name="endpointName">The name of the endpoint you have configured. By default, the value is '.auth'. The default redirect url is '/.auth/login/callback'.</param>
    /// <returns>The redirect uri the identity provider will return the auth code to.</returns>
    public string DetermineRedirectUri(PathString endpointName)
    {
        var hostName = DetermineHostName();
        return $"{hostName}{endpointName}/login/callback";
    }
}