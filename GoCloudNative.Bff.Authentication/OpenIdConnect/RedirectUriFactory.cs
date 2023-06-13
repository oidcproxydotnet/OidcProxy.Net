using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

internal class RedirectUriFactory : IRedirectUriFactory
{
    private readonly BffOptions _options;

    public RedirectUriFactory(BffOptions options)
    {
        _options = options;
    }
    
    /// <summary>
    /// Determines the current hostname.
    /// </summary>
    /// <param name="context">The current http context. Needed to determine the current host name.</param>
    /// <returns>The hostname the identity provider should return the auth code to.</returns>
    public string DetermineHostName(HttpContext context)
    {
        var hostName = _options.CustomHostName == null 
            ? new Uri($"{context.Request.Scheme}://{context.Request.Host}")
            : new Uri($"{_options.CustomHostName.Scheme}://{_options.CustomHostName.Authority}");

        return (hostName.Scheme == "http" && _options.AlwaysRedirectToHttps)
            ? $"https://{hostName.Authority}"
            : hostName.ToString().TrimEnd('/');
    }

    /// <summary>
    /// Determines where to redirect to. By default, this software will never redirect to a http address because the redirect url will contain sensitive information.
    /// </summary>
    /// <param name="context">The current http context. Needed to determine the current host name.</param>
    /// <param name="endpointName">The name of the endpoint you have configured. By default, the value is 'account'. The default redirect url is '/account/login/callback'.</param>
    /// <returns>The redirect uri the identity provider will return the auth code to.</returns>
    public string DetermineRedirectUri(HttpContext context, string endpointName)
    {
        var hostName = DetermineHostName(context);
        return $"{hostName}/{endpointName}/login/callback";
    }
}