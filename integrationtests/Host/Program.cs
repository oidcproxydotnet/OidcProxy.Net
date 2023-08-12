using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

namespace Host;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var redisConnectionString = builder.Configuration.GetSection("ConnectionStrings:Redis").Get<string>();
        ConfigureRedis(builder, redisConnectionString);
        
        var oidcConfig = builder.Configuration.GetSection("Oidc").Get<OpenIdConnectConfig>();
        var auth0Config = builder.Configuration.GetSection("auth0").Get<Auth0Config>();
        var aadConfig = builder.Configuration.GetSection("AzureAd").Get<AzureAdConfig>();
        
        ConfigureBff(builder, oidcConfig, auth0Config, aadConfig);

        var app = builder.Build();

        ConfigureApp(app, builder);

        app.Run();
    }

    public static void ConfigureRedis(WebApplicationBuilder builder, string? redisConnectionString)
    {
        if (string.IsNullOrEmpty(redisConnectionString))
        {
            return;
        }
        
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

    public static void ConfigureBff(WebApplicationBuilder builder, OpenIdConnectConfig? oidcConfig, Auth0Config? auth0Config, AzureAdConfig? aadConfig)
    {
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

            o.SetAuthenticationErrorPage("/account/oops");

            o.SetLandingPage("/account/welcome");

            //o.AddClaimsTransformation<MyClaimsTransformation>();

            o.ConfigureYarp(y =>
            {
                y.LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
                
                // suppress ssl errors (i should be ashamed of myself..) todo: make sure certificate is trusted in pipeline
                y.ConfigureHttpClient((_, h) =>
                    h.SslOptions.RemoteCertificateValidationCallback = (_, _, _, _) => true);
            });
        });

        builder.Services.AddLogging();
    }

    public static void ConfigureApp(WebApplication app, WebApplicationBuilder builder)
    {
        app.UseRouting();

        app.UseSecurityBff();

        // Test endpoints
        app.MapHealthChecks("/health");

        app.AddTestingEndpoints();
    }
}