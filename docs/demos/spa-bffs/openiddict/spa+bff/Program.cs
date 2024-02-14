using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.OpenIdConnect.Jwe;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

var key = new SymmetricSecurityKey(
    Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")
);

builder.Services.AddOidcProxy(config, o => o.UseJweKey(new EncryptionKey(key)));

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseOidcProxy();

app.Run();
