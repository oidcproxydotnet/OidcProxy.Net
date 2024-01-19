namespace OidcProxy.Net.ModuleInitializers;

public class BffConfig
{
    public string? EndpointName { get; set; }
    public string? ErrorPage { get; set; }
    public string? LandingPage { get; set; }
    public Uri? CustomHostName { get; set; }
    public string? SessionCookieName { get; set; }
    public TimeSpan? SessionIdleTimeout { get; set; }
    public YarpConfig? ReverseProxy { get; set; }
}