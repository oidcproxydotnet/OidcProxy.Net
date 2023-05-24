
---
author: Albert Starreveld
title: Implementing the BFF Security Pattern with AzureAd
description: Read how to implement the BFF Security Pattern with aspnetcore, Angular, and AzureAd
---
# Implementing the BFF Security Pattern with AzureAd

Complete the following three steps to implement the BFF Security Pattern with AzureAd:

1. Create an App Registration in Azure.
2. Create an aspnetcore API
3. Build a BFF

## Step 1.) Create an App Registration in Azure

To be able to authenticate users via Azure Active Directory, you must create an App Registration. Do so by executing the following commands:

```powershell
az ad app create --display-name bff --web-redirect-uris https://localhost:8443/account/login/callback https://localhost:8443/

az ad app credential reset --id {yourAppId} | ConvertFrom-Json
```

The output of these commands contains the client_id and the client_secret you need to configure the API and the BFF. 

Can't find them? Run this command:

```powershell
$app = az ad app create --display-name bff --web-redirect-uris https://localhost:8443/account/login/callback https://localhost:8443/ | ConvertFrom-Json

$credential = az ad app credential reset --id $app.appId | ConvertFrom-Json

$clientId = $app.appId;
$clientSecret = $credential.password;

Write-Host "--- Successfully created app registration ---"
Write-Host "ClientId: $clientId"
Write-Host "ClientSecret: $clientSecret"
```

Use the client_id and the client_secret to configure the BFF.


## Step 2.) Build the aspnetcore API

Create a new project:

```bash
dotnet new webapi
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package Microsoft.Identity.Web
```

Create the following `Program.cs` file:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(
        o => builder.Configuration.Bind("AzureAd", o), 
        o => builder.Configuration.Bind("AzureAd", o)
    );

// Add services to the container.
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
```

Make sure you have configured Azure in your `appsettings.json` file:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com",
    "ClientId": "{yourClientId}",
    "TenantId": "{yourTenantId}",
    "Audience": "{yourApplicationIdUri}"
  },
  "AllowedHosts": "*"
}
```

In this example, we assume you're running this API on port 8080. To get this API to run on that port, modify your `LaunchSettings.json` file to like like so:

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

To build a BFF with aspnet core, execute the following commands on the command line:

```bash
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.AzureAd
```

Create the following `Program.cs` file:

```csharp
using GoCloudNative.Bff.Authentication.AzureAd;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAzureAd(builder.Configuration.GetSection("AzureAd"));
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
  "AzureAd": {
    "ClientId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "TenantId": "{yourTenantId}",
    "DiscoveryEndpoint": "{https://login.microsoftonline.com/{tenantId}/v2.0/.well-known/openid-configuration}",
    "Scopes": [
      "openid", "profile", "offline_access", "https://yourDomain.onmicrosoft.com/test/api1"
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
To log a user in and to start a http session, navigate to `/account/login`. The software will redirect to the login page of the Identity Provider to log the user in. The resulting tokens will be stored in the user session and are not available in the browser.

### [GET] /account/login/callback
This endpoint is used by the IdentityProvider.

### [GET] /account/me
To see the logged in user, navigate to the `/account/me` endpoint. This endpoint shows the claims that are in the `id_token`.

### [GET] /account/end-session
To revoke the tokens that have been obtained when the user logged in, navigate to `/account/end-session` endpoint. This will revoke the tokens that have been stored in the user session. This will also end the user-session on at the Identity Provider

## Demo

Check out a fully working demo [here](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/AzureAd/src).

## Issues

Are you encountering issues? Please let us know at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues