using System.Net.Http.Headers;
using Bff;
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

// ======== Distributed session configuration ========
// Use this if you're deploying to a container platform like Azure Container Apps or Kubernetes.
// If the Redis connection string has been set, the following code configures two things
// 1. Instead of storing session variables in the memory, session variables will be stored in Redis now. This allows
//    the BFF to scale horizontally.
// 2. It adds data-protection, this means all the session variables that are stored in Redis will be encrypted
//
// This section is OPTIONAL. Although it is highly recommended to configure this, you can safely remove this section
// if it does not apply in your context.
// <begin>
var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
if (!string.IsNullOrEmpty(redisConnectionString))
{
    var redis = ConnectionMultiplexer.Connect(redisConnectionString);

    builder.Services
        .AddDataProtection()
        .PersistKeysToStackExchangeRedis(redis, "bff");

    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redis.Configuration;
        options.InstanceName = "bff";
    });
}
// <end>

// ========= Configure the BFF =========
// Reads the auth0 configuration from appsettings.json and bootstraps the BFF.
// This section is REQUIRED. Without this section, your BFF will not work. 
// <begin>
builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(builder.Configuration.GetSection("Auth0"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff(); 
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