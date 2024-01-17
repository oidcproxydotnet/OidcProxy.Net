using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Host;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
var aadConfig = builder.Configuration.GetSection("bff").Get<AzureAdBffConfig>();

builder.Services.AddBff(aadConfig, o =>
{
    o.AddClaimsTransformation<MyClaimsTransformation>();
    
    if (!string.IsNullOrEmpty(redisConnectionString))
    {
        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
        o.ConfigureRedisBackBone(connectionMultiplexer, "http_session_key", "bff");
    }
});

var app = builder.Build();

app.UseBff();

app.Run();
 