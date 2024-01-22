using System.Security.Claims;
using System.Text.Encodings.Web;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OidcProxy.Net.Middleware;

public sealed class OidcAuthenticationHandler : AuthenticationHandler<OidcAuthenticationSchemeOptions>
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    public const string SchemaName = "OidcProxy.Net";

    public OidcAuthenticationHandler(IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<OidcAuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var token = _httpContextAccessor.HttpContext.Session.GetAccessToken();
            if (token == null)
            {
                return Task.FromResult(AuthenticateResult.NoResult());
            }

            var payload = token.ParseJwtPayload()?.ToArray() ?? Array.Empty<KeyValuePair<string, object>>();
            if (!payload.Any())
            {
                throw new AuthenticationFailureException("Failed to authenticate. " +
                                                         "The access_token jwt does not contain any claims.");
            }

            var claims = payload
                .Select(x => new Claim(x.Key, x.Value?.ToString() ?? string.Empty))
                .ToArray();

            // todo: make configurable
            var claimsIdentity = new ClaimsIdentity(claims, SchemaName, "sub", "role");

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, SchemaName);

            var result = AuthenticateResult.Success(ticket);
            return Task.FromResult(result);
        }
        catch (Exception e)
        {
            return Task.FromResult(AuthenticateResult.Fail(e));
        }
    }
}