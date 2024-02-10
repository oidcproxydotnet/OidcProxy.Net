using System.Security.Claims;
using System.Text.Encodings.Web;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OidcProxy.Net.Middleware;

public sealed class OidcProxyAuthenticationHandler : AuthenticationHandler<OidcProxyAuthenticationSchemeOptions>
{
    private readonly ITokenParser _tokenParser;
    private readonly IHttpContextAccessor _httpContextAccessor;
    public const string SchemaName = "OidcProxy.Net";

    public OidcProxyAuthenticationHandler(ITokenParser tokenParser,
        IHttpContextAccessor httpContextAccessor,
        IOptionsMonitor<OidcProxyAuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder) : base(options, logger, encoder)
    {
        _tokenParser = tokenParser;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        try
        {
            if (_httpContextAccessor.HttpContext == null)
            {
                AuthenticateResult.NoResult();
            }

            var token = _httpContextAccessor.HttpContext.Session.GetAccessToken();
            if (token == null)
            {
                AuthenticateResult.NoResult();
            }

            var payload = await _tokenParser.ParseAccessTokenAsync(token)
                ?? _tokenParser.ParseAccessToken(token);
            
            var claims = payload
                .Select(x => new Claim(x.Key, x.Value?.ToString() ?? string.Empty))
                .ToArray();
            
            if (!claims.Any())
            {
                throw new AuthenticationFailureException("Failed to authenticate. " +
                                                         "The access_token jwt does not contain any claims.");
            }

            var nameClaim = _tokenParser.GetNameClaim();
            var roleClaim = _tokenParser.GetRoleClaim();
            
            var claimsIdentity = new ClaimsIdentity(claims, SchemaName, nameClaim, roleClaim);

            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
            var ticket = new AuthenticationTicket(claimsPrincipal, SchemaName);

            return AuthenticateResult.Success(ticket);
        }
        catch (Exception e)
        {
            return AuthenticateResult.Fail(e);
        }
    }
}