# GCN-O-e9ba6693bb0e

> GCN-O-e9ba6693bb0e: Unable to start GoCloudNative.Bff. Invalid client_id. Configure the client_id in the appsettings.json or program.cs file and try again.

The GoCloudNative BFF is an authentication gateway. As a result, you must configure an identity provider (correctly) for it to start.

To bootstrap the BFF, load the identity provider configuration from the `appsettings.json`:

```csharp

//...
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("Oidc");

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(config);
    
    //...
});
```

## How to reproduce this error

To reproduce the error, the `ClientId` needs to be missing:

```json
  "Oidc": {
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or empty:

```json
  "Oidc": {
    "ClientId": "",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or misspelled:

```json
  "Oidc": {
    "Client_Id": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or incorrect casing:

```json
  "Oidc": {
    "Cliendid": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

## Solution
Configure the `ClientId` correctly:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

and restart the BFF.