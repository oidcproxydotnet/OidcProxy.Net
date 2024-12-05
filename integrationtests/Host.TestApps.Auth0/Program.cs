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

app.MapGet("/", async context =>
{
    context.Response.WriteAsync("<html><body>" +
                                "<a href=\"/.auth/login\">login</a><br>" +
                                "<a href=\"/.auth/me\">me</a><br>" +
                                "<a href=\"/.auth/end-session\">sign out</a><br>" +
                                "</body></html>");
});

app.UseAuth0Proxy();

app.Run();