using Castle.Components.DictionaryAdapter.Xml;
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.Extensions.Configuration;

namespace Host.IntegrationTests;

public static class Configuration
{
    public static OpenIdConnectConfig GetOpenIdConnectConfig()
    {
        return new OpenIdConnectConfig
        {
            ClientId = "bff",
            ClientSecret = "secret",
            Authority = "https://idsvr.azurewebsites.net/"
        };
    }
    
    public static Auth0Config GetAuth0Config(IConfigurationRoot config)
    {
        return config.GetSection("auth0").Get<Auth0Config>() ?? new Auth0Config();
    }
}