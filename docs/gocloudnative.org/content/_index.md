---
author: Albert Starreveld
title: GoCloudNative.BFF - Documentation
description: Everything you need to know when you implement the GoCloudNative.BFF with your aspnetcore API and your Angular/NX single page application.
---

# GoCloudNative.Authentication.Bff

## Secure, scalable, easy to use, and free.

The GoCloudNative.Authenticatio.Bff is a authentication gateway based on YARP. Implementing it should not take more than a couple of minutes:

```bash
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.OpenIdConnect
```

`program.cs`

```csharp
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(builder.Configuration.GetSection("Oidc"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

app.Run();
```

`appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
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
    "Routes": {
      "spa": {
        "ClusterId": "spa",
        "Match": {
          "Path": "/{*any}"
        }
      },
      "api": {
        "ClusterId": "api",
        "Match": {
          "Path": "/api/{*any}"
        }
      },
    },
    "Clusters": {
      "spa": {
        "Destinations": {
          "spa": {
            "Address": "http://localhost:4200/"
          }
        }
      },
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

That's all! 

[Read more.](table-of-content)