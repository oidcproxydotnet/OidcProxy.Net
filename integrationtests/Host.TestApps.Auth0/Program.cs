using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Host.TestApps.Auth0;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("Bff")
    .Get<Auth0BffConfig>();

builder.Services.AddBff(config, o => o.AddAuthenticationCallbackHandler<TestAuthenticationCallbackHandler>());

var app = builder.Build();

app.UseBff();

app.Run();