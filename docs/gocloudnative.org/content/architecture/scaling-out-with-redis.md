---
author: Albert Starreveld
title: Scaling out with Redis
description: Implement a scalable GoCloudNative.Bff backed by Redis.
tags: ["dotnetcore", ".net", "BFF", "Redis", "Azure", "Kubernetes", "Microservices"]
---

# Scaling out with Redis

The GoCloudNative.Bff is the entry point of a web application. Potentially, it processes a lot of requests. In a cloud-native web application, ideally, when there are too many requests to handle, new instances will automatically be deployed until the system has enough capacity to process the requests.

Nowadays, autoscaling is easy to configure. The container platform will automatically deploy as many containers as need be.

Unfortunately, when you apply the BFF Security Pattern, it's not that simple. For scaling to be that easy, the application needs to be stateless and the GoCloudNative.Bff is not.

## Scaling out may cause issues

When you deploy another instance of the GoCloudNative.Bff without configuring it properly, the following situation will probably occur:

![Scaling problems](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/scaling-problem.png)

* When the user first navigates to the BFF, the user session is initiated. Some HTTP session values are set.
* When the user initiates the second HTTP request to the BFF, this request might very well be processed by another node. Without extra configuration, this node will not 'know' this session variable. As a result, the request will fail.

_Note: If you are trying to reproduce the test case in the picture, beware of this: by default, traffic is divided randomly between nodes. So, the fewer nodes you have, the longer it might take for a request to fail. To test this properly, be sure to execute several HTTP requests. Some will fail, some will not._

## How to scale a stateful web-app

The GoCloudNative.Bff supports scaling out. It uses the `Microsoft.AspNetCore.Session` to manage the session state. To scale an `aspnetcore` app out, the session must be backed by a Redis cache:

![Scaling out with Redis](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/sessions-backed-by-redis.png)

This picture shows the user session is no longer stored on a node. Instead, it's stored in a Redis cache which is available to all nodes. The following situation occurs:

* The user navigates to the BFF. 
* The BFF sets a session variable.
* The session variable is stored in the Redis cache
* The user initiates the second HTTP request
* The HTTP request is handled by another node, and it retrieves the session variables from Redis.

## How to configure HTTP Sessions backed by Redis

To configure this, deploy a Redis instance. There are various ways to do this:

* [Deploy Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-configure)
* [Deploy a Redis container](https://collabnix.com/how-to-setup-and-run-redis-in-a-docker-container/)

After you've set up the Redis Cache, you can start building the BFF. For the BFF to work properly, several NuGet packages are required. Execute the following commands to create the BFF:

```powershell
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.Auth0
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
dotnet add package Microsoft.AspNetCore.DataProtection.StackExchangeRedis
```

* The GoCloudNative.Bff needs to be hosted in an `aspnetcore` web application. To do so, create a new `.csproj` by typing `dotnet new web`.
* In this document, we demonstrate how to scale horizontally with a BFF that authenticates users via Auth0. This requires the `GoCloudNative.Bff.Authentication.Auth0` package. You might as well choose the `GoCloudNative.Bff.Authentication.OpenIdConnect` or the `GoCloudNative.Bff.Authentication.AzureAd` package.
* To configure Redis, use the `Microsoft.Extensions.Caching.StackExchangeRedis` package.
* To make the information in the cache unreadable to anything but the BFF, use the `Microsoft.AspNetCore.DataProtection.StackExchangeRedis` package.

Next, wire it. Write the following `program.cs`:

```csharp
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.AspNetCore.DataProtection;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Configure the Redis session storage
// <<begin>>
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
// <<end>>

builder.Services.AddHealthChecks();

// Configure the BFF to authenticate with Auth0
// <<begin>>
var auth0Config = builder.Configuration.GetSection("auth0");
builder.Services.AddSecurityBff(o =>
    {
        o.ConfigureAuth0(auth0Config);
        o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    });
// <<end>>

builder.Services.AddLogging();

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.MapHealthChecks("/health");

app.Run();
```

## Testing it locally
To see if you have configured everything correctly, do the following:

* Start a Redis cache
* Configure the `connectionstring` in the BFF
* Start the BFF
* Log in by navigating to the `/account/login` endpoint
* See the user details by navigating to the `/account/me` endpoint
* Restart the BFF
* See the user details by navigating to the `/account/me` endpoint
    * If the user details are still visible, you have successfully configured HTTP sessions backed by Redis
    * If the `/account/me` endpoint returns HTTP status code 404, you have not configured it correctly