# GCN-AZ-e9ba6693bb0e

> GCN-AZ-e9ba6693bb0e: Unable to start GoCloudNative.Bff. Invalid client_id. Configure the client_id in the appsettings.json or program.cs file and try again.

The GoCloudNative BFF is an authentication gateway. As a result, you must configure an identity provider (correctly) for it to start.

To bootstrap the BFF, load the identity provider configuration from the `appsettings.json`:

```csharp

//...
var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration.GetSection("Azure");

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAzureAd(config);
    
    //...
});
```

## How to reproduce this error

To reproduce the error, the `ClientId` needs to be missing:

```json
  "Oidc": {
    "ClientSecret": "{yourClientSecret}",
    "Authority": "https://{yourAuthority}",
    "TenantId": "{yourTenantId}",
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
    "TenantId": "{yourTenantId}",
    "Authority": "https://{yourAuthority}",
    "Scopes": [
      "openid", "profile", "offline_access"
    ]
  },
```

and restart the BFF.

### How to find the ClientId in Azure

To find the correct value for the `ClientId` variable, 

* navigate to the Azure Portal, navigate to Azure Active Directory, and click `App Registrations` in the menu on the left. 
* Select your app registrations or create one. (If you don't have an app registration yet, follow [the Azure Active Directory Quickstart](https://bff.gocloudnative.org/integration-manuals/quickstarts/azuread/quickstart/))
* This is what the overview page of an `App registration` looks like:
![App Registration overview page](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/integration-manuals/quickstarts/azuread/app-registration-overview.png)
* Copy the `Application (client) ID` value to the `appsettings.json` file.
