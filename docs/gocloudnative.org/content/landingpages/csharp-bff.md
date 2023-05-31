---
author: Albert Starreveld
title: How to build a BFF with ASPNETCORE
description: Read how to implement a BFF with C#
tags: ["C#", "csharp", "dotnet", ".net", "dotnetcore", "BFF", "back-end for front-end"]
---
# How to build a BFF with C#

To build a Back-end for Front-end with C#, you can choose three approaches:

## Use the `dotnet angular` template
The easiest way to build a BFF in the dotnet ecosystem, is to use the standard dotnet Angular template. 

This template creates a dotnet aspnetcore web project with an Angular app bootstrapped in it. This API runs at the server-side, and the SPA runs at the client-side. 

By simply adding the endpoints you need, by implementing a controller, you can forward these requests further downstream.

Use this approach when you are building a relatively simple web-application which is maintained by a small group of people.

## Build an ASPNETCORE Reverse Proxy with YARP. 
Choose for this option if you have a complex Single-Page Application, like an NX-application, and when you have no authentication requirement.

Use YARP to relay traffic to downstream API's. You can use Minimal-APIs to implement custom endpoints to invoke requests to multiple downstream APIs and aggregate the results.

To create a YARP based BFF-project, do the following:

```powershell
dotnet new web
dotnet add package Yarp.ReverseProxy
```

Make sure to have the following `program.cs` file:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable endpoint routing, required for the reverse proxy
app.UseRouting();

// Register the reverse proxy routes
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy();
});

app.MapGet("/custom-endpoint", () => {
    // Invoke downstream endpoints here, and aggregate the results..
});

app.Run();
```

Use the following `appsettings.json` to relay traffic to a downstream API:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    "Routes": {
      "route1": {
          "ClusterId": "weatherForecastApi",
          "Match": {
           "Path": "/weatherforecast/{**catch-all}"
          }
      }
    },
    "Clusters": {
      "weatherForecastApi": {
        "Destinations": {
          "weatherForecastApi/destination1": {
            "Address": "http://localhost:7352"
          }
        }
      }
    }
  }
}
```

## GoCloudNative.Authentication.Bff
Do you have a complex micro-front-end architecture? Do you have multiple teams working on the same site? Do you have an authentication requirement? Then, you might consider using GoCloudNative.Authentication.Bff.

The GoCloudNative.Authentication.Bff project utilizes YARP to relay traffic to downstream services. It has authentication endpoints built into it which are compatible with Azure Active Directory, Auth0, IdentityServer4, and any other OpenId Connect compliant authentication server.

To set up a GoCloudNative.Authentication.Bff project, do the following:

```powershell
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.OpenIdConnect
```

Create the following `program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// Enable endpoint routing, required for the reverse proxy
app.UseRouting();

// Register the reverse proxy routes
app.UseEndpoints(endpoints =>
{
    endpoints.MapReverseProxy();
});

app.MapGet("/custom-endpoint", () => {
    // Invoke downstream endpoints here, and aggregate the results..
});

app.Run();
```

And add the following section to your `appsettings.json`:

```json
{
  "Logging": {
    ...
  },
  "Oidc": {
    "ClientId": "{YourClientId}",
    "ClientSecret": "{YourClientSecret}",
    "Authority": "{YourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
  "AllowedHosts": "*",
  "ReverseProxy": {
    ...
  }
}
```

Read quickstarts to see how to configure the BFF, the APIs. Also refer to the quickstarts for working demos:

- [How to implement a C# BFF with Auth0](/integration-manuals/quickstarts/auth0/quickstart)
- [How to implement a C# BFF with Azure Active Directory](/integration-manuals/quickstarts/azuread/quickstart)
- [How to implement a C# BFF with IdentityServer4](/integration-manuals/quickstarts/identityserver4/quickstart)
