---
author: Albert Starreveld
title: Implementing the BFF Security Pattern with Auth0
description: Read how to implement the BFF Security Pattern with aspnetcore, Angular, and Auth0
tags: ["C#", "API", "aspnetcore", "OIDC", "access_tokens"]
---
# Implementing the BFF Security Pattern with Auth0

Complete the following three steps to implement the BFF Security Pattern with Auth0:

1. Configure Auth0.
2. Create an aspnetcore API
3. Build a BFF

## Step 1.) Configure Auth0

The GoCloudNatibe.Bff only supports the Authorization Code Flow with Proof Key for Client Exchange. That's why it is important to configure Auth0 in a specific way. 

Follow these steps to configure Auth0 correctly:

* Go to https://manage.auth0.com and sign in
* Go to the `Applications` section in the menu on the left-hand side and click `Applications`
* Click `+ Create application` in the right upper corner
* Provide a name for your app and select `Regular web applications
* Now, click settings, now you'll see the following section: ![client-id/secret](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/auth0/clientid-secret.png)

* Copy the client_id, the secret, and the authority into the `appsettings.json`, like so:

```json
{
  ...
  "Auth0": {
    "ClientId": "iuw4kjwkj34kj3",
    "ClientSecret": "kjh423j43jkh43jk2443jhsdfgs345te4th",
    "Domain": "example.eu.auth0.com",
    "Audience": "https://example.eu.auth0.com/api/v2",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  }
  ...
}
```

* Now, configure the `redirect_url`. When the user has logged into Auth0, Auth0 will redirect the user to this URL. Redirecting will not work unless the redirect URL has been whitelisted: ![Whitelisting the redirect_uri](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/auth0/redirect-uri.png)

* Next, scroll to the `Advanced settings` and configure the `grant_types`. Enable `Authorization Code` and `Refresh tokens` ![grant-types](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/auth0/grant-types.png)

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
    .AddJwtBearer(options =>
    {
        options.Authority = $"https://{builder.Configuration["Auth0:Domain"]}/";
        options.Audience = builder.Configuration["Auth0:Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = ClaimTypes.NameIdentifier
        };
    });

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
```

Make sure you have configured Auth0 in your `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Auth0": {
    "Domain": "{yourDomain}",
    "Audience": "{yourApiIdentifier}"
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
dotnet add package GoCloudNative.Bff.Authentication.Auth0
```

Create the following `Program.cs` file:

```csharp
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(builder.Configuration.GetSection("Auth0"));
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
  "Auth0": {
    "ClientId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Domain": "{yourDomain}",
    "Audience": "{yourAudience}",
    "FederatedLogout": false,
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

Use the following `Properties/launchSettings.json`, this launchSettings file ensures the application url matches the callback url that has been configured in Auth0:

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

Check out a fully working demo [here](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/Auth0/src).

## Issues

Are you encountering issues? Please let us know at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues