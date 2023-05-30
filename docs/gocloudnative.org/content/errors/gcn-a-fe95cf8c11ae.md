# GCN-A-fe95cf8c11ae

> GCN-A-fe95cf8c11ae: Unable to start GoCloudNative.Bff. Invalid audience. Configure the audience in the appsettings.json or program.cs file and try again.

The GoCloudNative BFF is a authentication gateway. As a result, you must configure an identity provider (correctly) in order for it to start.

To bootstrap the BFF, it's recommended to load the identity provider confiuration from the `appsettings.json`:

```csharp

//...
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("Auth0");

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(config);
    
    //...
});
```

In order to get an access token with a payload, you need to provide a value for `audience`.

## How to reproduce this error

To reproduce the error, the `Audience` needs to be missing:

```json
  "Auth0": {
    "ClientId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Domain": "{yourDomain}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

Or empty

```json
  "Auth0": {
...
    "Audience": "",
...
  },
```

Or misspelled

```json
  "Auth0": {
...
    "Audienc": "{yourDomain}",
...
  },
```

## Solution

Configure the domain correctly:

```json
  "Auth0": {
...
    "Audience": "https://www.mydomain.com/api/test",
...
  },
```

To find the correct value for `audience`

- Go to https://manage.auth0.com
- Under `Applications`, in the menu on the left, click `APIs`
- Select your API or create one
- Next, you'll see the following screen: ![](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/auth0/apis.png)
- Copy the `Identifier` to appsettings.json