using System.Security.Claims;
using System.Text.Encodings.Web;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OidcProxy.Net.Jwt;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Middleware;

internal sealed class OidcProxyAuthenticationHandler(
    ITokenParser tokenParser,
    ProxyOptions proxyOptions,
    IHttpContextAccessor httpContextAccessor,
    IOptionsMonitor<OidcProxyAuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder)
    : AuthenticationHandler<OidcProxyAuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string SchemaName = "OidcProxy.Net";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (httpContextAccessor.HttpContext == null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = httpContextAccessor.HttpContext.Session.GetAccessToken();
            if (string.IsNullOrEmpty(token))
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var payload = tokenParser.ParseJwtPayload(token);
            if (payload == null)
            {
                throw new AuthenticationFailureException("Failed to authenticate. " +
                                                         "The access_token jwt does not have a payload.");
            }
            
            var claims = payload
                .Select(x => new Claim(x.Key, x.Value?.ToString() ?? string.Empty))
                .ToArray();
            
            if (!claims.Any())
            {
                throw new AuthenticationFailureException("Failed to authenticate. " +
                                                         "The access_token jwt does not contain any claims.");
            }

            var claimsIdentity = new ClaimsIdentity(claims, SchemaName, proxyOptions.NameClaim, proxyOptions.RoleClaim);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, SchemaName);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
        catch (Exception e)
        {
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }
}