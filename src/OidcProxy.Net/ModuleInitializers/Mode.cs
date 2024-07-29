namespace OidcProxy.Net.ModuleInitializers;

public enum Mode
{
    /// <summary>
    /// Requires configuring Yarp. Requests will be forwarded downstream using the provided Yarp configuration.
    /// </summary>
    Proxy,
    /// <summary>
    /// Authenticates users only. Does not forward requests downstream automatically.
    /// </summary>
    AuthenticateOnly
}