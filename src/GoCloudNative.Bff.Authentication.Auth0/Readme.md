# GoCloudNative.Bff.Authentication.Auth0

This package contains the software you need to implement the BFF Security Pattern. This software does three things:

1. It manages the user session
2. It allows the user to log into the site
3. It forwards request to downstream services and adds the Authorization header with the user's access token to the requests

The GoCloudNative BFF is a stateful reverse proxy. To forward requests to downstream services GoCloudNative BFF uses YARP.

Currently, GoCloudNative BFF supports logging in with Azure, Auth0, IdentityServer4, and any other OpenID Connect compliant authorization server. Currently, only the Authorization Code flow with Proof-Key Client Exchange is supported.

## Quickstart: Implementing the BFF Security Pattern

To build a BFF, execute the following commands:

```bash
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication.Auth0
```

Create the following `Program.cs` file:

```csharp
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("Bff")
    .Get<Auth0BffConfig>();

builder.Services.AddBff(config);

var app = builder.Build();

app.UseBff();

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
  "AllowedHosts": "*",
  "Bff": {
    "Auth0": {
      "ClientId": "{yourClientId}",
      "ClientSecret": "{yourClientSecret}",
      "Domain": "{yourDomain}",
      "Audience": "{yourAudience}",
      "Scopes": [
        "openid",
        "profile",
        "email"
      ]
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

In this example we assume you are running a Single Page Application on localhost on port `4200` and you have an API running at localhost on port `8080`. If that is not the case, then update the `appsettings.json` accordingly.

To run the BFF, type `dotnet run` or just hit the 'play'-button in Visual Studio.

## Endpoints

The BFF relays all requests as configured in the `ReverseProxy` section in the `appsettings.json` file, except for four endpoints:

### [GET] /account/login
To log a user in and to start a http session, navigate to `/account/login`. The software will redirect to the login page of the Identity Provider to log the user in. The resulting tokens will be stored in the user session and are not available in the browser.

### [GET] /account/login/callback
This endpoint is used by the IdentityProvider.

### [GET] /account/me
To see the logged in user, navigate to the `/account/me` endpoint. This endpoint shows the claims that are in the `id_token`.

### [GET] /account/end-session
To revoke the tokens that have been obtained when the user logged in, execute a get request on the `/account/end-session` endpoint. This will revoke the tokens that have been stored in the user session and will not log the user out from the Identity Provider session. This must be implemented at client side.

## Issues

Are you encountering issues? Please let us know at: https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues