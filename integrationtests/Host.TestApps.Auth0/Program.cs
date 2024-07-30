using Host.TestApps.Auth0;
using OidcProxy.Net.Auth0;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<Auth0ProxyConfig>();

builder.Services.AddAuth0Proxy(config!, o => o.AddAuthenticationCallbackHandler<TestAuthenticationCallbackHandler>());

var app = builder.Build();

app.MapGet("/custom/me", async context =>
    {
        var identity = (ClaimsIdentity)context.User.Identity!;
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

app.UseAuth0Proxy();

app.Run();