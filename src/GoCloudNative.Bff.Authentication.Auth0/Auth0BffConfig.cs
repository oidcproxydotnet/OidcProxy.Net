using GoCloudNative.Bff.Authentication.ModuleInitializers;

namespace GoCloudNative.Bff.Authentication.Auth0;

public class Auth0BffConfig : BffConfig
{
    public Auth0Config Auth0 { get; set; }
}