using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
    {
        o.ConfigureOpenIdConnect(builder.Configuration.GetSection("Oidc"));
        o.ConfigureAuth0(builder.Configuration.GetSection("auth0"), "auth0");
        o.ConfigureAzureAd(builder.Configuration.GetSection("AzureAd"), "aad");
        
        o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    });

builder.Services.AddLogging();

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();
