using OidcProxy.Net.Auth0;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<Auth0ProxyConfig>();

builder.Services.AddAuth0Proxy(config);

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.UseAuth0Proxy();

app.Run();
