using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

builder.Services.AddOidcProxy(config);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseOidcProxy();

app.Run();
