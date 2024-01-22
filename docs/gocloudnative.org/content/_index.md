---
author: Albert Starreveld
title: OidcProxy.Net - A scalable, free OIDC Authentication Gateway
description: Elevate the security of your web application to the most up-to-date standards by incorporating the BFF Security Pattern using OidcProxy.Net.
---

## Why?

The development of our product was driven by our clients' need for a straightforward authentication gateway. Existing market options introduced complexities in terms of pricing and licensing, or proved to be overly intricate for beginners to comprehend and utilize effectively.

Consequently, organizations are forced to make a trade-off between maintainability and security. In today's automated society, this compromise is unacceptable.

Hence, our mission is to offer an affordable, developer-friendly, and secure BFF Framework that can be implemented by anyone.

## Getting started

You can swiftly implement the [BFF Security Pattern](https://bff.gocloudnative.org/concepts/bff-security-pattern/) within minutes using the OidcProxy.Net, an authentication gateway built on YARP. Just follow these steps to get started:

```bash
dotnet new web
dotnet add package OidcProxy.Net.OpenIdConnect
```

`program.cs`

```csharp
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

builder.Services.AddOidcProxy(config);

var app = builder.Build();

app.UseOidcProxy();

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
  "AllowedHosts": "*",
  "OidcProxy": {
    "Oidc": {
      "ClientId": "{YourClientId}",
      "ClientSecret": "{YourClientSecret}",
      "Authority": "{YourAuthority}",
    },
    "ReverseProxy": {
      "Routes": {
        "api": {
          "ClusterId": "api",
          "Match": {
            "Path": "/api/{*any}"
          }
        }
      },
      "Clusters": {
        "api": {
          "Destinations": {
            "api/node1": {
              "Address": "http://localhost:8080/"
            }
          }
        }
      }
    }
  }
}

```

To gain a comprehensive understanding of API security, reverse proxies, horizontal scaling or the implementation process of the OidcProxy.Net, we recommend referring to [our documentation](/table-of-contents/). It provides detailed information and guidance on these topics.