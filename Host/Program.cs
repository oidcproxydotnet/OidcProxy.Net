using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Host;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var oidcConfig = builder.Configuration.GetSection("Oidc").Get<OpenIdConnectConfig>();
var auth0Config = builder.Configuration.GetSection("auth0").Get<Auth0Config>();
var aadConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();

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

builder.Services.AddHealthChecks();

builder.Services.AddSecurityBff(o =>
    {
        if (oidcConfig != null)
        {
            o.ConfigureOpenIdConnect(oidcConfig, "oidc");
        }

        if (auth0Config != null)
        {
            o.ConfigureAuth0(auth0Config, "auth0");
        }

        if (aadConfig != null)
        {
            o.ConfigureAzureAd(aadConfig, "aad");
        }

        o.AddClaimsTransformation<MeEndpointClaimsTransformation>();
        o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    });

builder.Services.AddLogging();

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.MapHealthChecks("/health");

app.Run();
