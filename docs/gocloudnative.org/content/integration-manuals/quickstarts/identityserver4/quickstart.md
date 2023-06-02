---
author: Albert Starreveld
title: Implementing the BFF Security Pattern with IdentityServer4
description: Read how to implement the BFF Security Pattern with aspnetcore, Angular, and IdentityServer4
tags: ["API", "aspnetcore", "OIDC", "access_tokens"]
---
# Implementing the BFF Security Pattern with IdentityServer4

Complete the following three steps to implement the BFF Security Pattern with IdentityServer4:

1. Configure IdentityServer.
2. Create an aspnetcore API
3. Build a BFF

## Step 1.) Configure IdentityServer4

The GoCloudNatibe.Bff only supports the Authorization Code Flow with Proof Key for Client Exchange. That's why it is important to configure IdentityServer in a specific way. Configure the Client as follows:

```csharp

    public static readonly Client Client = new Client
    {
        // Set the ClientId and the ClientSecret
        ClientId = "bff",
        ClientSecrets =
        {
            new Secret("secret".Sha256())
        },

        // Configure the Authorization Code flow with PKCE
        AllowedGrantTypes = GrantTypes.Code,
        RequirePkce = true,

        // Configure the access token lifetime (1h by default)
        AccessTokenLifetime = 3600,

        // Make sure IdentityServer may redirect to the bff
        RedirectUris = { "https://localhost:8443/account/login/callback" }, 
        FrontChannelLogoutUri = "https://localhost:8443/",
        PostLogoutRedirectUris = { "https://localhost:8443/" },

        // Enable offline access, the BFF needs it to refresh the tokens
        // after they expire. Otherwise, the session would end after 1h.
        AllowOfflineAccess = true,

        // Add the profile claims to the id token so they are available
        // in the /account/me endpoint of the BFF.
        AlwaysIncludeUserClaimsInIdToken = true,
        AllowedScopes = 
        {
            IdentityServerConstants.StandardScopes.OpenId,
            IdentityServerConstants.StandardScopes.Profile,
            IdentityServerConstants.StandardScopes.Email,
            // .. other scopes
        }
    };
```

Find a sample identity-server implementation here: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/Integration-Manuals/Integrating-With-Identity-Providers/IdentityServer4/src/IdentityServer4


## Step 2.) Build the aspnetcore API

Create a new project:

```bash
dotnet new webapi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
```

Create the following `Program.cs` file:

```csharp
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = builder.Configuration["Oidc:Authority"];

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateAudience = false
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Make sure you have configured IdentityServer in your `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Oidc": {
    "Authority": "https://{yourAuthority}"
  },
  "AllowedHosts": "*"
}
```

In this example, we assume you're running this API on port 8080. To get this API to run on that port, modify your `LaunchSettings.json` file to like so:

```json
{
  "profiles": {
    "http": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "launchUrl": "/",
      "applicationUrl": "http://localhost:8080",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

## Step 3.) Build the BFF

To build a BFF with `aspnetcore`, execute the following commands on the command line:

```bash
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.OpenIdConnect
```

Create the following `Program.cs` file:

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

Create the following `appsettings.json` file:

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

Use the following `Properties/launchSettings.json`, this launchSettings file ensures the application URL matches the callback URL that has been configured in IdentityServer:

```json
{
  "profiles": {
    "https": {
      "commandName": "Project",
      "dotnetRunMessages": true,
      "launchBrowser": true,
      "applicationUrl": "https://localhost:8443",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      }
    }
  }
}
```

In this example we assume you are running a Single Page Application on localhost on port `4200` and you have an API running at localhost on port `8080`. If that is not the case, then update the `appsettings.json` accordingly.

To run the BFF, type `dotnet run` or just hit the 'play'-button in Visual Studio. When you run the BFF, make sure to have your API and your SPA running too.

### Endpoints

The BFF relays all requests as configured in the `ReverseProxy` section in the `appsettings.json` file, except for four endpoints:

### [GET] /account/login
To log a user in and to start an HTTP session, navigate to `/account/login`. The software will redirect to the login page of the Identity Provider to log the user in. The resulting tokens will be stored in the user session and are not available in the browser.

### [GET] /account/login/callback
This endpoint is used by the IdentityProvider.

### [GET] /account/me
To see the logged-in user, navigate to the `/account/me` endpoint. This endpoint shows the claims that are in the `id_token`.

### [GET] /account/end-session
To revoke the tokens that have been obtained when the user logs in, navigate to `/account/end-session` endpoint. This will revoke the tokens that have been stored in the user session. This will also end the user-session on at the Identity Provider

## Demo

Check out a fully working demo [here](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/IdentityServer4/src).

## Issues

Are you encountering issues? Please let us know at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues