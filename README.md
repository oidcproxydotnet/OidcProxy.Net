# OidcProxy.Net

![](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/actions/workflows/ci.yml/badge.svg)
![Nuget](https://img.shields.io/nuget/dt/OidcProxy.Net)
![Version](https://img.shields.io/nuget/v/OidcProxy.Net)
[![Twitter](https://shields.io/twitter/follow/oidcproxydotnet)](https://twitter.com/intent/follow?screen_name=oidcproxydotnet)

## Deployment Options

- [Do it yourself](###scaffold-a-proxy)
- [Docker Compose](https://github.com/oidcproxydotnet/OidcProxy.Net/blob/main/docs/demos/Authentication-Gateways/Auth0/src/readme.md)
- [Kubernetes]()

### Scaffold a proxy:

```bash
# Download and install the template pack first
dotnet new install OidcProxy.Net.Templates

# Scaffold the proxy
dotnet new OidcProxy.Net --backend "https://api.myapp.com"
    --idp "https://idp.myapp.com"
    --clientId xyz
    --clientSecret abc

# Run it
dotnet run
```

### Do it yourself:

```bash
dotnet new web
dotnet add package OidcProxy.Net.OpenIdConnect
```

`Program.cs`:

```csharp
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<OidcProxyConfig>();

builder.Services.AddOidcProxy(config);

// Or, in case of an identity provider that uses Json Web Encryption:
// var key = new SymmetricSecurityKey(
//     Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")
// );
// 
// builder.Services.AddOidcProxy(config, o => o.UseJweKey(new EncryptionKey(key)));

var app = builder.Build();

app.UseOidcProxy();

app.Run();
```

`appsettings.json`:

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
    "LandingPage": "/hello",
    "Oidc": {
      "ClientId": "clientid",
      "ClientSecret": "secret",
      "Authority": "https://login.yoursite.com/"
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

## What is OidcProxy.Net?
The OidcProxy is designed to enhance security by keeping the `access_token` and the `refresh_token` hidden from the browser while still allowing the proxy itself to handle and use these tokens. It includes them in downstream requests. This approach helps mitigate potential security risks associated with token exposure.

#### OidcProxy.Net in a nutshell
- The OidcProxy serves as an identity-aware proxy.
- Authentication initiates a session on the OidcProxy.
- End-users authenticate through an Open ID Connect Identity Provider.
- The requests forwarded by the OidcProxy include `access_tokens` in the requests to backend services.
- The OidcProxy is a Nuget package that can be included in .net 8 projects.

#### Token Visibility and Security Measures:
- The `access_token`, `id_token`, and `refresh_token` are not visible to the browser. This enhances security by preventing these sensitive tokens from being exposed to potential attackers via the browser.

#### Token Handling by OidcProxy:
- While the tokens are not visible to the browser, the OidcProxy itself does have access to these tokens.
- The OidcProxy adds an `Authorization=Bearer [ACCESS_TOKEN]` header to each downstream request.

#### Authorization Code with PKCE Confidential Client Grant:
- The OidcProxy enables the implementation of the OAuth2 Authorization Code Grant with Confidential Client Grant.
- This reduces the risk of impersonation.
- This reduces the attack surface because, in this scenario, an attacker who does not have access to the webserver cannot be issued any tokens. 

## Features

OidcClient.Net has the following features:

- By requiring minimal configuration, OidcProxy.Net enables authentication through the following Identity Providers:
  - KeyCloak 
  - Identity Server ([How to](https://medium.com/web-security/implementing-the-bff-security-pattern-with-identityserver4-and-gocloudnative-bff-a8b594308363))
  - Auth0 ([How to](https://medium.com/web-security/implementing-the-bff-security-pattern-with-gocloudnative-bff-and-auth0-773a888979c))
  - Azure EntraId ([How to](https://medium.com/web-security/implementing-the-bff-security-pattern-with-azuread-b2c-4f340cafecfb))
  - OpenIddict
- Leveraging YARP, OidcProxy.Net allows for sophisticated routing configurations, enabling users to define rules for directing incoming requests to the appropriate backend services.
- The proxy can run in single instance mode or in disributed mode. In distributed mode, the proxy uses Redis as a backbone.
- Authentication is integrated into the ASP.NET Core pipeline. This allows for the usage of the `Authorization` attribute, Policies, and many other ASP.NET Identity feature
- The proxy has been built following SOLID principles. This allows developers to extend the basic functionality to tailor it to their needs.
- OidcProxy.Net can easily included in
  - .NET Web Projects
  - .NET Web API Projects
  - .NET Angular projects
  - .NET React projects
- The proxy is designed for Docker, but native execution is also supported.
- Authenticate users with JWT and JWE.

## How it works

The OidcProxy does not just forward traffic to down-stream services, it adds the `Authentication` header to the forwarded requests too. This is illustrated in the following diagram:

<p align="center">
    <img src="BFF-flow.png" width="300">
</p>

The user also uses the proxy to initiate the authentication procedure. That's done by navigating to a special endpoint: the `/.auth/login` endpoint.

This process is visualised in the following diagram:

<p align="center">
  <img src="BFF-login-workflow.png">
</p>
  
## Why we built it
The development of our product was driven by our clients’ need for a straightforward authentication gateway. Existing market options introduced complexities in terms of pricing and licensing, or proved to be overly intricate for beginners to comprehend and utilize effectively.

Consequently, organizations are forced to make a trade-off between maintainability and security. In today’s automated society, this compromise is unacceptable.

Hence, our mission is to offer an affordable, developer-friendly, secure, identity-aware BFF Framework that can be implemented by anyone.

## Feedback

We need your feedback. Like any other software product, it is impossible to be successful without user-feedback. Please take a moment of your time to fill out this form: [https://forms.gle/a6uuwFSLSAod52MH9](https://forms.gle/a6uuwFSLSAod52MH9)
