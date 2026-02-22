using OidcProxy.Net.ModuleInitializers;

// ReSharper disable once CheckNamespace
namespace OidcProxy.Net;

public class ProxyConfig : IAppSettingsSection
{
    public Mode Mode { get; set; }
    public string? EndpointName { get; set; }
    public string? ErrorPage { get; set; }
    public string? LandingPage { get; set; }
    public string? NameClaim { get; set; }
    public string? RoleClaim { get; set; }
    public IEnumerable<string> AllowedLandingPages { get; set; } = Array.Empty<string>();
    public bool EnableUserPreferredLandingPages { get; set; } = false;
    public bool? AllowAnonymousAccess { get; set; }
    public Uri? HostName { get; set; }
    public string? CookieName { get; set; }
    public TimeSpan? SessionIdleTimeout { get; set; }
    public YarpConfig? ReverseProxy { get; set; }

    public virtual bool Validate(out IEnumerable<string> errors)
    {
        errors = Array.Empty<string>();
        return true;
    }

    public virtual void Apply(ProxyOptions options)
    {
        AssignIfNotNull(ErrorPage, options.SetAuthenticationErrorPage);
        AssignIfNotNull(LandingPage, options.SetLandingPage);
        AssignIfNotNull(CookieName, cookieName => options.CookieName = cookieName);
        AssignIfNotNull(NameClaim, nameClaim => options.NameClaim = nameClaim);
        AssignIfNotNull(RoleClaim, roleClaim => options.RoleClaim = roleClaim);
        
        options.HostName = HostName 
                           ?? throw new NotSupportedException("GCN-O-1700faa58fdf: Unable to start OidcProxy.Net. Invalid hostname. " +
                                                              "Configure the hostname in the appsettings.json or program.cs file and try again. " +
                                                              "More info: https://github.com/oidcproxydotnet/OidcProxy.Net/wiki/Errors#gcn-o-1700faa58fdf");
        
        options.Mode = Mode;
        options.EnableUserPreferredLandingPages = EnableUserPreferredLandingPages;
        options.AllowAnonymousAccess = !AllowAnonymousAccess.HasValue || AllowAnonymousAccess.Value;
        options.EndpointName = EndpointName ?? ".auth";
        options.SetAllowedLandingPages(AllowedLandingPages);

        if (SessionIdleTimeout.HasValue)
        {
            options.SessionIdleTimeout = SessionIdleTimeout.Value;
        }
        
        if (options.Mode != Mode.AuthenticateOnly)
        {
            var routes = ReverseProxy?.Routes.ToRouteConfig();
            var clusters = ReverseProxy?.Clusters.ToClusterConfig();

            options.ConfigureYarp(routes, clusters);
        }
    }
    
    private static void AssignIfNotNull<T>(T? value, Action<T> @do)
    {
        if (value != null)
        {
            @do(value);
        }
    }
}