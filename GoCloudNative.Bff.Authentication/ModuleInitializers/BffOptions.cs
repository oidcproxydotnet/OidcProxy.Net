using System.Text.RegularExpressions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class BffOptions
{
    internal readonly IdpRegistrations IdpRegistrations = new();
    
    internal Action<IReverseProxyBuilder> ApplyReverseProxyConfiguration = _ => { };

    internal Action<IServiceCollection> ApplyClaimsTransformation = (s) => s.AddTransient<IClaimsTransformation, DefaultClaimsTransformation>();
    
    internal Uri? CustomHostName = null;

    internal string? ErrorPage = null;

    /// <summary>
    /// The name of the cookie
    /// </summary>
    public string SessionCookieName { get; set; } = "bff.cookie";
    
    public bool AlwaysRedirectToHttps { get; set; } = true;

    /// <summary>
    /// Sets a custom page to redirect to when the authentication on the OIDC Server failed.
    /// The url will be augmented with an additional query string parameter to indicate what error occured.
    /// </summary>
    /// <param name="errorPage">A relative path to the error page</param>
    public void SetAuthenticationErrorPage(string errorPage)
    {
        const string errorMessage = "Invalid error page. The path to the error page must be relative and may not have a querystring.";

        if (string.IsNullOrEmpty(errorPage))
        {
            throw new NotSupportedException(errorMessage);
        }
        
        Uri path;
        try
        {
            path = new Uri(errorPage, UriKind.Relative);
        }
        catch (Exception)
        {
            throw new NotSupportedException(errorMessage);
        }
        
        if (path.IsAbsoluteUri || errorPage.Contains('?') || errorPage.Contains('#'))
        {
            throw new NotSupportedException(errorMessage);
        }

        ErrorPage = errorPage;
    }

    /// <summary>
    /// The GoCloudNative.BFF typically derives the redirect URL from the request context as a default behavior. However, in cases where the hosting of an image with the GoCloudNative.BFF involves proxies or different configurations, the automatically inferred redirect URL may be incorrect. To address this issue, you can utilize the following method to override the default value of the redirect URL.
    /// </summary>
    /// <param name="hostname"></param>
    /// <exception cref="NotSupportedException"></exception>
    public void SetCustomHostName(Uri hostname)
    {
        if (!string.IsNullOrEmpty(hostname.Query))
        {
            throw new NotSupportedException("GCN-B-322cf6ab8a70: " +
                                            "Cannot initialize GoCloudNative.BFF. " +
                                            "Error configuring custom hostname. " +
                                            $"{hostname} is not a valid hostname. " +
                                            "A custom hostname may not have a querystring.");
        }

        CustomHostName = hostname;
    }

    public void RegisterIdentityProvider<TIdentityProvider, TOptions>(TOptions options, string endpointName = "account") 
        where TIdentityProvider : class, IIdentityProvider 
        where TOptions : class
    {
        IdpRegistrations.Register<TIdentityProvider, TOptions>(options, endpointName);
    }

    /// <summary>
    /// By default, the /{0}/me endpoint displays the payload of the ID token, including all the claims. However, there may be situations where it is necessary to display fewer claims or additional claims are required. To customize the claims shown in the /{0}/me endpoint, you can create a custom implementation of the IClaimsTransformation interface and register it using this method. This allows you to control the transformation and selection of claims for the endpoint. 
    /// </summary>
    /// <typeparam name="TClaimsTransformation">The class that augments the output of the /{0}/me endpoint</typeparam>
    public void AddClaimsTransformation<TClaimsTransformation>() where TClaimsTransformation : class, IClaimsTransformation
    {
        ApplyClaimsTransformation = s => s.AddTransient<IClaimsTransformation, TClaimsTransformation>();
    }

    /// <summary>
    /// Initialize YARP with the values provided in a configuration-section.
    /// </summary>
    public void LoadYarpFromConfig(IConfigurationSection configurationSection)
    {
        ApplyReverseProxyConfiguration = b => b.LoadFromConfig(configurationSection);
    }

    /// <summary>
    /// YARP is initially set up to forward traffic based on the predefined configuration. However, if you require additional configuration options, you can utilize this method to extend the configuration.
    /// </summary>
    public void ConfigureYarp(Action<IReverseProxyBuilder> configuration)
    {
        ApplyReverseProxyConfiguration = configuration;
    }
}