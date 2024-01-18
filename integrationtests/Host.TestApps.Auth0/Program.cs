using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Host.TestApps.Auth0;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("Bff")
    .Get<Auth0BffConfig>();

builder.Services.AddBff(config, 
    o =>
    {
        o.AddAuthenticationCallbackHandler<TestAuthenticationCallbackHandler>();
        
        var conn = ConnectionMultiplexer.Connect("localhost");
        o.ConfigureRedisBackBone(conn, "foo bazr");
    });

var app = builder.Build();

app.UseBff();

app.Run();