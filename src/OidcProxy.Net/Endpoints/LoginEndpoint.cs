using OidcProxy.Net.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Endpoints;

internal static class LoginEndpoint
{
    public static async Task Get(HttpContext context,
            [FromServices] ProxyOptions options,
            [FromServices] ILogger<IIdentityProvider> logger,
            [FromServices] IRedirectUriFactory redirectUriFactory,
            [FromServices] IIdentityProvider identityProvider)
    {
        try
        {
            if (options.EnableUserPreferredLandingPages || options.AllowedUserPreferredLandingPages.Any())
            {
                try
                {
                    await SaveUserPreferredLandingPage(context, options.AllowedUserPreferredLandingPages);
                }
                catch (Exception e)  when (e is ArgumentException or NotSupportedException)
                {
                    var userPreferredLandingPage = context.Request.Query["landingpage"];
                    logger.LogWarning(context, $"Suspicious activity detected. User provided an invalid landing page at " +
                                               $"login. Value provided: \"{userPreferredLandingPage}\"");
                    
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    return;
                }
            }

            var endpointName = context.Request.Path.RemoveQueryString().TrimEnd("/login");
            
            var redirectUri = redirectUriFactory.DetermineRedirectUri(context, endpointName);

            var authorizeRequest = await identityProvider.GetAuthorizeUrlAsync(redirectUri);

            if (!string.IsNullOrEmpty(authorizeRequest.CodeVerifier))
            {
                await context.Session.SetCodeVerifierAsync(authorizeRequest.CodeVerifier);
            }

            logger.LogLine(context, $"Redirect({authorizeRequest.AuthorizeUri})");

            context.Response.Redirect(authorizeRequest.AuthorizeUri.ToString());
        }
        catch (Exception e)
        {
            logger.LogException(context, e);
            throw;
        }
    }

    private static async Task SaveUserPreferredLandingPage(HttpContext context, LandingPage[] allowedLandingPages)
    {
        var userPreferredLandingPage = context.Request.Query["landingpage"].ToString();

        if (context.Request.Path
            .RemoveQueryString()
            .Equals(userPreferredLandingPage, StringComparison.InvariantCultureIgnoreCase))
        {
            throw new NotSupportedException($"Will not redirect user to {userPreferredLandingPage}");
        }

        // If there is a whitelist of landing pages configured in the config, and if the user preferred landing page
        // is not in it, throw an exception
        if (allowedLandingPages.Any() 
            && !string.IsNullOrEmpty(userPreferredLandingPage) 
            && !allowedLandingPages.Any(x => x.Equals(userPreferredLandingPage)))
        {
            throw new NotSupportedException($"Will not redirect user to {userPreferredLandingPage}");
        }

        await context.Session.SetUserPreferredLandingPageAsync(userPreferredLandingPage);
    }
}