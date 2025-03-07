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
    public string DetermineHostName(HttpContext context)
    {
        Uri hostName;
        if (options.CustomHostName == null)
        {
            hostName = new Uri($"{context.Request.Scheme}://{context.Request.Host}");
        }
        else
        {
            var host = options.CustomHostName.Authority;
            var localPath = options.CustomHostName.LocalPath.TrimEnd('/');
            hostName = new Uri($"{options.CustomHostName.Scheme}://{host}{localPath}");
        }
        
        return (hostName.Scheme == "http" && options.AlwaysRedirectToHttps)
            ? $"https://{hostName.Authority}"
            : hostName.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Determines where to redirect to. By default, this software will never redirect to a http address because the redirect url will contain sensitive information.
    /// </summary>
    /// <param name="context">The current http context. Needed to determine the current host name.</param>
    /// <param name="endpointName">The name of the endpoint you have configured. By default, the value is '.auth'. The default redirect url is '/.auth/login/callback'.</param>
    /// <returns>The redirect uri the identity provider will return the auth code to.</returns>
    public string DetermineRedirectUri(HttpContext context, PathString endpointName)
    {
        var hostName = DetermineHostName(context);
        return $"{hostName}{endpointName}/login/callback";
    }
}