using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(builder.Configuration.GetSection("IdentityProvider"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();
