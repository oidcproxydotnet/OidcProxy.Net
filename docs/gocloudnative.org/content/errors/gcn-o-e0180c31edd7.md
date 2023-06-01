# GCN-O-e0180c31edd7

> GCN-O-e0180c31edd7: Unable to start GoCloudNative.Bff. Invalid authority. Configure the authority in the appsettings.json or program.cs file and try again.

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

To reproduce the error, the `Authority` needs to be missing:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or empty:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or misspelled:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Autority": "https://yourauthority.com",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

or incorrect casing:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "authority": "https://yourauthority.com",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

Most importantly: The Authority MUST be a valid URL. If you provide a value that isn't a URL, this exception will be thrown too.

These are invalid Authority values:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "authority": "yourauthority.com",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

and

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "authority": "htp://yourauthority.com",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

## Solution
Configure the `Authority` correctly:

```json
  "Oidc": {
    "CliendId": "{yourClientId}",
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://yourauthority.com",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

and restart the BFF.