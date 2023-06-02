---
author: Albert Starreveld
title: How to set up an authenticated C# BFF that aggregates the responses of downstream requests.
description: A BFF is meant to delegate requests downstream and aggregate the results. When these APIs require authentication and when the authentication is handled by the BFF, than the BFF must include the access_token in the downstream requests. This article describes how this works and how to implement it.
tags: ["C#", "csharp", "dotnet", ".net", "dotnetcore", "BFF", "back-end for front-end", "authentication", "authorization", "access_token", "microservices"]
---

# Using your BFF as an aggregation service

The BFF pattern was created to reduce the number of back-end calls a front-end needs to execute. The front-end does so by delegating the downstream-requests and the aggregation of the results to the BFF.

Also, the BFF acts as an anti-corruption layer. When the downstream services change, that does not necessarily require a contract-change between front-end and BFF.

## A BFF in a Microservices Architecture

Introducing the BFF Pattern in a microservices architecture creates the following situation:

![Bff Aggregation Service](https://miro.medium.com/v2/resize:fit:720/format:webp/1*7qkKsi2RpsbWT3ChnYW6Vw.png)

* Each front-end has its own Back-end for Front-end
* Microservices are not exposed to the front-ends directly, all traffic is proxied through the BFF
* Requests to microservices and their responses can be augmented by the BFF

## Using the GoCloudNative.BFF as an authenticated request aggregator

The GoCloudNative.BFF can be used as a request aggregator. This is demonstrated in the [demos](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos). The demo APIs have two endpoints:

* /api/weatherforecast/usa
* /api/weatherforecast/sahara

The front-end SPA in the demo app executes 3 requests:

* /api/weatherforecast
* /api/weatherforecast/usa
* /api/weatherforecast/sahara

The `/api/weatherforecast` is processed by the BFF. It invokes the downstream weatherforecast api to collect data from `/api/weatherforecast/usa` and `/api/weatherforecast/sahara` and it aggregates the results.

## Aggregating responses from multiple endpoints

In abstract, a BFF:

* Accepts HTTP-requests, invokes multiple downstream services to fulfill the request, and aggregates the results 
* Forwards incoming requests to downstream APIs

To forward requests to downstream services, directly, use YARP. To implement an endpoint which aggregates results, implement an `aspnetcore` API. 

To invoke downstream services, chances are, you need an `access_token`. This token is stored in the HTTP-session and can be obtained by using the `HttpContext.Current.Session.GetAccessToken()` method. This method is made available to you via the `GoCloudNative.Bff.Authentication.OpenIdConnect` namespace in case of OpenId Connect, via the `GoCloudNative.Bff.Authentication.Auth0` namespace in case of Auth0, and the `GoCloudNative.Bff.Authentication.AzureAd` namespace in case of Azure Ad. You must include the token in the downstream requests manually. 

## A BFF-example that aggregates downstream HTTP-requests

You can implement an aggregation service as follows:

`program.cs`

```csharp
using System.Net.Http.Headers;
using Bff;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(builder.Configuration.GetSection("Oidc"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

// This is an example how you can execute multiple requests 
app.Map("/api/weatherforecast", async (HttpContext context, HttpClient httpClient) =>
{
    var accessToken = context.Session.GetAccessToken();
    if (string.IsNullOrEmpty(accessToken))
    {
        context.Response.StatusCode = 401;
        return;
    }

    httpClient.DefaultRequestHeaders.Authorization 
       = new AuthenticationHeaderValue("Bearer", accessToken);

    var usa = await httpClient.GetAsStringAsync("http://localhost:8080/api/weatherforecast/usa");
    var sahara = await httpClient.GetAsStringAsync("http://localhost:8080/api/weatherforecast/sahara");

    await context.Response.WriteAsJsonAsync("{ " +
                                            $"\"usa\": {usa}, " +
                                            $"\"sahara\": {sahara} " +
                                            "}");
});

app.Run();
```

Use the following `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Oidc": {
    "ClientId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "api": {
        "ClusterId": "api",
        "Match": {
          "Path": "/api/{*any}"
        }
      },
    },
    "Clusters": {
      "api": {
        "Destinations": {
          "api": {
            "Address": "http://localhost:8080/"
          }
        }
      },
    }
  }
}
```

This code forwards all requests to `http://localhost:8080`, via YARP. Except the `/api/weatherforecast` endpoint. This endpoint has been mapped explicitly, so YARP skips it. 

In the controller method, the `access_token` is obtained and added to requests to downstream services with the following code:

```csharp
var accessToken = context.Session.GetAccessToken();
httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
```

Check out fully working examples here:

- [IdentityServer4](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/IdentityServer4/src)
- [Auth0](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/Auth0/src)
- [AzureAd](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/AzureAd/src)

## Cost of a BFF as an Aggregation Service

Every architectural decision comes at a price. Depending on context it either makes sense to choose such an approach or not. In a web-context, there are two common use cases where BFFs are applied:

1. One Single-Page Application with one or several back-end APIs.
2. A micro-front-end architecture with multiple micro-front-ends, a micro-frontend host, and several APIs. 

Assuming scenario 1, consider the following cost:

* Implementing a back-end call will, in some cases, require changes in the BFF. This means the front-end team and the BFF-team will have to work closely together. Or it means the front-end team must maintain both the SPA and the BFF which will require them to learn both front-end programming languages (like TypeScript or Javascript) and back-end programming languages (C#).
* This approach does not scale well with large amounts of data. Assume you are building a webshop. Assume a list of 100 orders. They come from the order-service and this service does not contain any product-information. As a result, to display the name of the ordered product, the BFF must first query the order-service and then query the product-service for each and every order to get the product-name.

Assuming scenario 2, consider the following cost:

* Since, in a micro-front-end context, there are several front-ends, there must be one team who is "owner" of the BFF. When a micro-front-end requires aggregating responses of multiple downstream services, these changes must be implemented in the BFF.
    * Who will implement these changes?
    * This means a deployment of a micro-service and a micro-service might require a new version of the BFF to be delpoyed too
* This approach does not scale well with large amounts of data.
