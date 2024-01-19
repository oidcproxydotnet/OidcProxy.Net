using Yarp.ReverseProxy.Configuration;

namespace OidcProxy.Net.ModuleInitializers;

public class YarpConfig
{
    public Dictionary<string, RouteConfig> Routes { get; set; } = new();
    public Dictionary<string, ClusterConfig> Clusters { get; set; } = new();
}