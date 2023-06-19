---
author: Albert Starreveld
title: Implementing the BFF Security Pattern with AzureAd
---
# Implementing the BFF Security Pattern with AzureAd (B2c)

Complete the following three steps to implement the BFF Security Pattern with AzureAd:

1. Create an App Registration in Azure.
2. Create an aspnetcore API
3. Build a BFF

## Step 1.) Create an App Registration in Azure (B2c)

To be able to authenticate users via Azure Active Directory, you must create an App Registration. Go to https://portal.azure.com, and follow these steps:

- Create an app registration
- Create a secret
- Create an Application ID URI and a scope
- Request API permissions for the scope, and grant it

### 1. Create an app registration
* Navigate to `Azure Active Directory`, and click `App registrations` in the menu on the left (or click [here](https://portal.azure.com/#view/Microsoft_AAD_IAM/ActiveDirectoryMenuBlade/~/RegisteredApps)).
* Click `+ New registration`, and fill out the form as displayed in the screenshot: ![](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/new-app-registration.png)\
Make sure to register the `redirect_url` correctly. Usually, that's something like `https://{where-you-host-the-bff}/account/login/redirect`. You can add multiple values here. Be sure to ___never register a localhost `redirect_url` for your production environment!!!___
* When you have completed the form, click the `Register` button at the bottom of the form.
* Now, you see the following overview: ![App registration overview](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/app-registration-overview.png)
  * Important: Copy the `application ID` to `appsettings.json`. This is the `ClientId`.
  * Important: Copy the `Directory ID` to `appsettings.json`. This is the `TenantId`.

### 2. Create a secret
* Click `Add a certificate or secret`, and create a new secret: ![New secret](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/new-client-secret.png)\
Copy the `secret` to the `appsettings.json`. This is the `ClientSecret`.

### 3. Create an application ID URI
* Now, click `Add an Application ID URI` to create a scope. You'll see the following page: ![New App ID URI](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/adding-scope-1.png)
* Click `add a scope`: ![Create a new scope](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/adding-scope-2.png)
* When you click `Add scope`, Azure will ask for an Application URI first: ![Create app URI](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/adding-scope-3.png)\
Choose any name you like here.
* After the `scope` has been created successfully, you'll be redirected to the `Expose an API` overview page: ![](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/added-scope.png)\
Copy the URL that's displayed here, copy it to the `appsettings.json`, to the `scopes` section.

### 4. Request permission and grant consent
* Now, navigate to `API Permissions` in the main menu on the left. You will see this screen: ![](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/initial-permissions.png)
* Click `+ Add a permission`, and fill out the form like so: ![Add permission](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/request-api-permissions.png)
* Now, the permission will be added to the list. But it hasn't been approved yet. Click `Grant admin consent for B2c` ![Grant consent](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/approve-permission.png)
* As a result of all of the above, you'll need the following section in your BFFs `appsettings.json`:

```json
  "AzureAd": {
    "ClientId": "abaedd26-ba6f-4d45-a123-415a23b32756",
    "ClientSecret": "xkd8Q~UkPKI-wc2_AMvxmXyL-152I0JF4PSZZdpb",
    "TenantId": "983356be-7fda-4f41-887c-a6a87e9fcf34",
    "DiscoveryEndpoint": "https://login.microsoftonline.com/983356be-7fda-4f41-887c-a6a87e9fcf34/v2.0/.well-known/openid-configuration",
    "Scopes": [
      "openid", "profile", "offline_access", "https://example.onmicrosoft.com/api1/weatherforecast.read"
    ]
  },
```


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
To see the logged-in user, navigate to the `/account/me` endpoint. This endpoint shows the claims that are in the `id_token`.

### [GET] /account/end-session
To revoke the tokens that have been obtained when the user logs in, navigate to `/account/end-session` endpoint. This will revoke the tokens that have been stored in the user session. This will also end the user-session on at the Identity Provider

## Demo

Check out a fully working demo [here](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/tree/main/docs/demos/AzureAd/src).

## Feedback

Help us build better software. Your feedback is valuable to us. We would like to inquire about your success in setting up this demo.

- Were you able to successfully set up a BFF with Azure Active Directory? Please share your thoughts on the overall experience by answering the questions in our [feedback form](/feedback/).
- Did you face any difficulties or encounter missing features? Kindly inform us at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues
