using GoCloudNative.Bff.Authentication.OpenIdConnect;

namespace Host.IntegrationTests;

public static class Configuration
{
    public static OpenIdConnectConfig GetOpenIdConnectConfig()
    {
        return new OpenIdConnectConfig
        {
            ClientId = "bff",
            ClientSecret = "secret",
            Authority = "https://localhost:7185"
        };
    }
}