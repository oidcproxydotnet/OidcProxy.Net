using System.Security.Claims;
using OidcProxy.Net.EntraId;
using OidcProxy.Net.ModuleInitializers;
using Host;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

var entraIdConfig = builder.Configuration
    .GetSection("OidcProxy")
    .Get<EntraIdProxyConfig>();

builder.Services.AddEntraIdProxy(entraIdConfig, o =>
{
    o.AddClaimsTransformation<MyClaimsTransformation>();
    o.ConfigureRedisBackBone(connectionMultiplexer);
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

app.UseEntraIdProxy();

app.Run();
 