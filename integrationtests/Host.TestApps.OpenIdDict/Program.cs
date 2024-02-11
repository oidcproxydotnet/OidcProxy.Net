using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.OpenIdConnect.Jwe;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

builder.Services
    .AddOidcProxy(config, o =>
    {
        var cert = X509Certificate2.CreateFromPemFile("cert.pem", "key.pem");
        o.ConfigureJweEncryptionKey(new Certificate(cert));
     
        // var key = new SymmetricSecurityKey(
        //     Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")
        // );
        // o.ConfigureJweEncryptionKey(new EncryptingCredentials(key));
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