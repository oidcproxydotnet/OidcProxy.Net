using System.Linq;
using System.Security.Claims;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

builder.Services.AddOidcProxy(config);

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

app.UseOidcProxy();

app.Run();
