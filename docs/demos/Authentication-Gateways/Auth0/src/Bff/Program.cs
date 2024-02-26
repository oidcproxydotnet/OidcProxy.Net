using System.Net.Http.Headers;
using Bff;
using OidcProxy.Net;
using OidcProxy.Net.Auth0;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();

var runLocallyWithDocker = builder.Configuration.GetSection("runLocallyWithDocker").Get<bool>();

// ========= Configure the BFF =========
// Reads the auth0 configuration from appsettings.json and bootstraps the BFF.
// This section is REQUIRED. Without this section, your BFF will not work. 
// <begin>
var auth0Config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<Auth0ProxyConfig>();

builder.Services.AddAuth0Proxy(auth0Config, o =>
{
    if (runLocallyWithDocker)
    {
        o.AlwaysRedirectToHttps = false;
        o.SetCustomHostName(new Uri("http://localhost:8443"));
    }

    if (string.IsNullOrEmpty(redisConnectionString))
    {
        return;
    }
    
    var connection = ConnectionMultiplexer.Connect(redisConnectionString);
    o.ConfigureRedisBackBone(connection);
});

var app = builder.Build();

app.UseRouting();

app.UseAuth0Proxy();
// <end>

// ========= Create an endpoint that delegates requests to multiple downstream services =========
// This section is OPTIONAL. You can safely remove it if it does not apply in your context.
// <begin>
app.Map("/api/weatherforecast", async (HttpContext context, HttpClient httpClient) =>
{
    var accessToken = context.Session.GetAccessToken();
    if (string.IsNullOrEmpty(accessToken))
    {
        context.Response.StatusCode = 401;
        return;
    }

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var apiBaseAddress = builder.Configuration
        .GetSection("ReverseProxy:Clusters:api:Destinations:api:Address")
        .Get<string>()
        .EnsureUrlEndsWithSlash();

    var usa = await httpClient.GetAsStringAsync($"{apiBaseAddress}api/weatherforecast/usa");
    var sahara = await httpClient.GetAsStringAsync($"{apiBaseAddress}api/weatherforecast/sahara");

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("{ " +
                                        $"\"usa\": {usa}, " + 
                                        $"\"sahara\": {sahara} " +
                                      "}");
});
// <end>

app.Run();