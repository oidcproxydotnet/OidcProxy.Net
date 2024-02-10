using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.OpenIdDict;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OpenIdDictProxyConfig>();

builder.Services
    .AddOpenIdDictProxy(config)
    .AddValidation(options =>
    {
        // <don't use this in production!>
        // Create your own signing key instead! Or choose another encryption method...
        options.AddEncryptionKey(new SymmetricSecurityKey(
            Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY="))
        );
        // </don't use this in production!>
    });

var app = builder.Build();

app.UseOpenIdDictProxy();

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