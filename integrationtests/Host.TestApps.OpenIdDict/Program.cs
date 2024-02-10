using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.OpenIdConnect.Jwe;
using OidcProxy.Net.OpenIdDict;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OpenIdDictProxyConfig>();

builder.Services
    .AddOidcProxy(config, o =>
    {
        var cert = X509Certificate2.CreateFromPemFile("cert.pem", "key.pem");
        o.ConfigureJwe(new Certificate(cert));
    });

var app = builder.Build();

app.UseOidcProxy();

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

app.Run();