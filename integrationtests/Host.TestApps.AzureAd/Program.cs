using System.Security.Claims;
using OidcProxy.Net.AzureAd;
using OidcProxy.Net.ModuleInitializers;
using Host;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

var aadConfig = builder.Configuration.GetSection("bff").Get<AzureAdBffConfig>();

builder.Services.AddBff(aadConfig, o =>
{
    o.AddClaimsTransformation<MyClaimsTransformation>();
    o.ConfigureRedisBackBone(connectionMultiplexer, "http_session_key");
});

var app = builder.Build();

app.MapGet("/custom/me", async context =>
    {
        var identity = (ClaimsIdentity)context.User.Identity;
        await context.Response.WriteAsJsonAsync(new
        {
            Sub = identity.Name,
            Claims = identity.Claims.Select(x => new
            {
                Type = x.Type,
                Value = x.Value
            })
        });
    })
    .RequireAuthorization();

app.UseBff();

app.Run();
 