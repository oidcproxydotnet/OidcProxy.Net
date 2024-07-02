using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

var key = new SymmetricSecurityKey(
         Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")
     );

builder.Services.AddOidcProxy(config, o => o.UseEncryptionKey(new SymmetricKey(key)));

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