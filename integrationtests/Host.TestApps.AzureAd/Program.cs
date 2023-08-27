using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Host;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();

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

var aadConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();

builder.Services.AddHealthChecks();

builder.Services.AddBff(o =>
{
    o.ConfigureAzureAd(aadConfig, "aad");

    o.SetAuthenticationErrorPage("/account/oops");
    o.SetLandingPage("/account/welcome");

    o.AddClaimsTransformation<MyClaimsTransformation>();
    
    o.ConfigureYarp(y =>
    {
        y.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    });
});

builder.Services.AddLogging();
var app = builder.Build();

app.UseRouting();

app.UseBff();

// Test endpoints
app.MapHealthChecks("/health");

app.Run();
 