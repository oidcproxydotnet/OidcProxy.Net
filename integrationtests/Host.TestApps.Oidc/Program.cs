using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("Bff")
    .Get<OidcBffConfig>();

builder.Services.AddBff(config);

var app = builder.Build();

app.UseBff();

app.Run();
