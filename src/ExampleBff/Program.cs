using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(builder.Configuration.GetSection("Oidc"));
    //o.ConfigureAuth0(builder.Configuration.GetSection("auth0"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();
