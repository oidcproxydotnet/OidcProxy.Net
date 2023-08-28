using Yarp.ReverseProxy.Configuration;

namespace GoCloudNative.Bff.Authentication.ModuleInitializers;

public class YarpConfig
{
    public Dictionary<string, RouteConfig> Routes { get; set; }
    public Dictionary<string, ClusterConfig> Clusters { get; set; }
}