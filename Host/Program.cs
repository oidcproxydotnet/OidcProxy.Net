using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var oidcConfig = builder.Configuration.GetSection("Oidc");
var auth0Config = builder.Configuration.GetSection("auth0");
var aadConfig = builder.Configuration.GetSection("AzureAd");

builder.Services.AddSecurityBff(o =>
    {
        if (oidcConfig != null)
        {
            o.ConfigureOpenIdConnect(oidcConfig, "oidc");
        }

        if (auth0Config != null)
        {
            o.ConfigureAuth0(auth0Config, "auth0");
        }

        if (aadConfig != null)
        {
            o.ConfigureAzureAd(aadConfig, "aad");
        }

        o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    });

builder.Services.AddLogging();

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();
