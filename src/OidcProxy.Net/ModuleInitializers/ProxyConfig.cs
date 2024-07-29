namespace OidcProxy.Net.ModuleInitializers;

public class ProxyConfig
{
    public Mode Mode { get; set; }
    public string? EndpointName { get; set; }
    public string? ErrorPage { get; set; }
    public string? LandingPage { get; set; }
    public string? NameClaim { get; set; }
    public string? RoleClaim { get; set; }
    public IEnumerable<string> AllowedLandingPages { get; set; }
    public bool EnableUserPreferredLandingPages { get; set; } = false;
    public bool? AlwaysRedirectToHttps { get; set; }
    public bool? AllowAnonymousAccess { get; set; }
    public Uri? CustomHostName { get; set; }
    public string? CookieName { get; set; }
    public TimeSpan? SessionIdleTimeout { get; set; }
    public YarpConfig? ReverseProxy { get; set; }
}