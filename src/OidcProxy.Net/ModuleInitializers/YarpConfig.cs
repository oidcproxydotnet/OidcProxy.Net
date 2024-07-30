using Yarp.ReverseProxy.Configuration;

// ReSharper disable once CheckNamespace
namespace OidcProxy.Net;

public class YarpConfig
{
    public Dictionary<string, RouteConfig> Routes { get; set; } = new();
    public Dictionary<string, ClusterConfig> Clusters { get; set; } = new();
}