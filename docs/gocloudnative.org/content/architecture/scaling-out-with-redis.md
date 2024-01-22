---
author: Albert Starreveld
title: Scaling out with Redis
description: Implement a scalable OidcProxy.Net backed by Redis.
---

# Scaling out with Redis

The OidcProxy.Net proxy acts as the primary gateway for a web application. It is commonly utilized in microservices architectures hosted on container platforms like Kubernetes or Azure Container Apps, which support automatic scaling by deploying additional instances to handle increased request loads.

Nevertheless, implementing the BFF Security Pattern introduces complexities to scaling. The application cannot be fully stateless, impeding the straightforward auto-scaling features typically provided by container platforms.

## The problem

If more than one instance of the OidcProxy.Net proxy is deployed without proper configuration, it is likely that the following situation will arise:

![Scaling problems](https://raw.githubusercontent.com/thecloudnativewebapp/OidcProxy.Net/main/docs/gocloudnative.org/content/Diagrams/scaling-problem.png)

* Upon the user's initial navigation to the BFF, the user session is initiated, and certain HTTP session values are established.
* However, when the user makes a subsequent HTTP request to the BFF, it may be processed by a different node. Without additional configuration, this node will not be aware of the session variables, leading to a failed request.

_Note: If you are attempting to replicate the test case depicted in the image, please be cautious of the following: by default, traffic is randomly distributed among nodes. Consequently, with fewer nodes, it may take longer for a request to fail. To conduct a thorough test, make sure to execute multiple HTTP requests, as some will fail while others will not._

## How to scale a stateful web-app

The OidcProxy.Net is designed to facilitate scaling out and utilizes `Microsoft.AspNetCore.Session` to manage session state. To effectively scale out an `aspnet core` app, it is crucial to configure the session to be backed by a Redis cache.

![Scaling out with Redis](https://raw.githubusercontent.com/thecloudnativewebapp/OidcProxy.Net/main/docs/gocloudnative.org/content/Diagrams/sessions-backed-by-redis.png)

In this picture, the user session is no longer stored on a specific node but rather in a Redis cache that is accessible to all nodes. As a result, the following scenario unfolds:

* The user navigates to the BFF. 
* The BFF sets a session variable.
* The session variable is stored in the Redis cache
* The user initiates the second HTTP request
* The HTTP request is handled by another node, and it retrieves the session variables from Redis.

## How to configure HTTP Sessions backed by Redis

To set up this configuration, you need to deploy a Redis instance, and there are multiple methods available to achieve this, including:

* [Deploy Azure Cache for Redis](https://learn.microsoft.com/en-us/azure/azure-cache-for-redis/cache-configure)
* [Deploy a Redis container](https://collabnix.com/how-to-setup-and-run-redis-in-a-docker-container/)

Once the Redis Cache is set up, you can proceed with building the BFF. You will need to install several required NuGet packages. 

Execute the following commands to create the BFF:

```powershell
dotnet new web
dotnet add package OidcProxy.Net.Auth0
```

* The OidcProxy.Net proxy needs to be hosted in an `aspnetcore` web application. To do so, create a new `.csproj` by typing `dotnet new web`.
* In this document, we demonstrate how to scale horizontally with a BFF that authenticates users via Auth0. This requires the `OidcProxy.Net.Auth0` package. You might as well choose the `OidcProxy.Net.OpenIdConnect` or the `OidcProxy.Net.EntraId` package.
* To configure Redis, use the `Microsoft.Extensions.Caching.StackExchangeRedis` package.
* To make the information in the cache unreadable to anything but the BFF, use the `Microsoft.AspNetCore.DataProtection.StackExchangeRedis` package.

Next, wire it. Write the following `program.cs`:

```csharp
using OidcProxy.Net.Auth0;
using OidcProxy.Net.ModuleInitializers;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

var auth0Config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<Auth0ProxyConfig>();

builder.Services.AddAuth0Proxy(auth0Config, o =>
{
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