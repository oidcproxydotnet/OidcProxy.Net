---
author: Albert Starreveld
title: GoCloudNative.BFF - A free, developer-friendly Backend-For-Frontend Security Framework
description: Elevate the security of your web application to the most up-to-date standards by incorporating the BFF Security Pattern using GoCloudNative.BFF.
---

# GoCloudNative.Bff

Secure, scalable, developer-friendly, and free.

## Our mission

The development of our product was driven by our clients' need for a straightforward authentication gateway. Existing market options introduced complexities in terms of pricing and licensing, or proved to be overly intricate for beginners to comprehend and utilize effectively.

Consequently, organizations are forced to make a trade-off between maintainability and security. In today's automated society, this compromise is unacceptable.

Hence, our mission is to offer an affordable, developer-friendly, and secure BFF Framework that can be implemented by anyone.

## Getting started

You can swiftly implement the [BFF Security Pattern](https://bff.gocloudnative.org/concepts/bff-security-pattern/) within minutes using the GoCloudNative.Bff, an authentication gateway built on YARP. Just follow these steps to get started:

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

To gain a comprehensive understanding of API security, reverse proxies, horizontal scaling or the implementation process of the GoCloudNative BFF, we recommend referring to [our documentation](table-of-contents). It provides detailed information and guidance on these topics.

## We need your support

GoCloudNative.BFF is an open-source project, emphasizing its community-driven nature. It is a free product and will continue to be freely accessible. Safeguarding the World Wide Web is a collective endeavor, necessitating your assistance. You can contribute by:

* [Sharing your valuable feedback](/feedback)
* [Contributing to our GitHub project](/github)
* Writing about the GoCloudNative.BFF, spreading awareness