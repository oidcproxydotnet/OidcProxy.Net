using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Host;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
var aadConfig = builder.Configuration.GetSection("bff").Get<AzureAdBffConfig>();

if (!string.IsNullOrEmpty(redisConnectionString))
{
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);

    builder.Services
        .AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "bff");

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis.Configuration;
        options.InstanceName = "bff";
    });
}

builder.Services.AddBff(aadConfig, o =>
{
    o.AddClaimsTransformation<MyClaimsTransformation>();
});

var app = builder.Build();

app.UseBff();

app.Run();
 