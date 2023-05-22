# Use a secure Back-end For Frontend

This repository contains a Back-end For Front-end written in C#. It authenticates users, it forwards requests to down-stream API's and adds the access_token to the forwarded requests by adding the `Authentication` header with value `Bearer xyz` to it. You can also use this BFF as a BFF is intended to be used: to invoke down-stream endpoints 'manually' and aggregate the results.

## !! Work in progress !!

_This software is still in an expirimental phase. Please take some time to test this software and to provide feedback (by creating [issues](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/issues)!). The more feedback we get, the faster we can release a 1.0.0 version._

## Getting started

To implement the BFF Security Pattern, execute the following commands:

```bash
dotnet new web
dotnet add package GoCloudNative.Bff.Authentication
dotnet add package GoCloudNative.Bff.Authentication.OpenIdConnect
```

Create the following `Program.cs`:

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

And use the following `appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Oidc": {
    "ClientId": "[InsertClientIdHere]",
    "ClientSecret": "[InsertClientSecretHere]",
    "Authority": "https://authority",
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


This solution supports authentication with

* Auth0 _([Read documentation](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/blob/main/src/GoCloudNative.Bff.Authentication.Auth0/Readme.md))_
* ~~Okta~~ (todo..)
* ~~Azure Active Directory~~ (todo..)
* Identity Server
* ~~Key Cloak~~ (todo..)

# What problem does this software solve?

Many organizations who have a Single Page Application, have access_tokens in the front-end, and have a microservices architecture, are now struggling to move authentication to the server side. This is made possible with this software.

This section explains:
* What is 'wrong' with having access_tokens at the client-side
* What the alternative is
* How this software works

## The problem:

Many organisations have implemented the following architecture:

<p align="center">
    <img src="docs/diagrams/spa-without-bff.png" alt="PKCE without client_secret" width="500">
</p>

* The Single Page Application
    * It runs in the browser
    * It takes initiative to authenticate the user by forwarding unauthenticated users to the Identity Provider
    * When the user has authenticated, the Single Page Application typically takes initiative to let this authentication manifest in an access token (by invoking the /token endpoint)
    * The Single Page Application invokes http endpoints with a `Authentication` header which contains the token.
* The Identity Provider
    * This is an OpenId Connect server
    * It produces access tokens
    * It validates the users identity. In other words: Users log in here.
* APIs/microservices
    * Protected with access_tokes. Typically, the resources in this API can only be accessed with a valid access_token.
    * The APIs apply a rule to determine whether a user is authorized to use/see a resource or not. In other words: It applies a policy.

## What is 'wrong' with this architecture?

There are two reasons one could argue not to use this architecture:

### 1.) Token exchange in the front-end exposes an unnecessary attack vector

In such an architecture, when the user authenticates, a code is being sent to the front-end. The front-end must then exchange this code for an access_token.

Theoretically, in this scenario, there is no way of telling who will exchange the code for a token. To ensure access tokens are being sent to the intended recipient, the client_secret is being used. In this scenario, the client secret is _not_ being used.

### 2.) The access token can be 'stolen'
Theoretically, because the token is being stored at the client side, it is easier to steal a token.

### __Don't worry. This might not be a problem for you!__
The fact that this architecture has these weak spots does not mean it is not a 'secure' architecture. Determine whether or not your application is secure enough for you by determining how likely it is your application will be compromised. Next, determine how much damage can be caused if somebody manages to steal the access token. 

Use the probability/impact matrix to see if you need to act.

## The recommended solution:

Often, to mitigate the risks associated with the architecture stated in the previous chapter, the proposed solution is to move the authentication to the server side.

As a result, the solution architecture will be a lot more complex. (This is why you must carefully consider whether or not you really need this.)

To make it possible to authenticate the user on the server side, you'll need a component which keeps track of the user's session and authentictes the user:

<p align="center">
    <img src="docs/diagrams/architecture.png" alt="PKCE with client_secret on the server side" width="500">
</p>

In this diagram there is a:

* Single Page App at the server side
    * Runs in the browser
    * It runs on the same domain as the API's
    * It does not take initiative to authenticate users
* The BFF
    * Serves the resources for the Single Page Application (index.html and the /dist folder)
    * Exposes the API's
    * Has a HTTP-Session
    * Takes initiative to authenticate the user (by redirecting the user to the Identity Provider)
* The Identity Provider
    * This is an OpenId Connect server
    * It produces access tokens
    * It validates the users identity. In other words: Users log in here.
* APIs/microservices
    * Protected with access_tokes. Typically, the resources in this API can only be accessed with a valid access_token.
    * The APIs apply a rule to determine whether a user is authorized to use/see a resource or not. In other words: It applies a policy.

### How is this more secure?

This approach is more secure because of two reasons.

__1.) The token exchange is done at the server side__ 

For an attacker there is no way to see how the BFF obtains an access token. Therefor, it is extremely hard to interfere.

Also, when authentication is done at the server side, it is secure to include the client_secret in the token request. This means the Identity Provider is now able to validate who is asking for a token.

__2.) More secure session management__

When a token is stored in the browser, typically this is done by storing the token in a secure cookie, in local storage, or in session storage.

When the attacker manages to copy these, the attacker basically hijacked the session. To prevent this, the front-end would need to implement all sorts of measures to prevent session hijacking, cross-site request forgery, and so forth. 

The best way to implement session management is by not implementing it yourself. Microsoft (and many other major corporations) has done the work for you. By running a session on the server side you can leverage the asp.net framework to get a proper http session.

### How does it work?

Simply put, a BFF is nothing but a reverse proxy. But it does not just forward traffic to down-stream services, it adds the `Authentication` to the forwarded requests too. 

This BFF is built to process two types of requests. Most of the requests are API requests done by a Single Page Application. A BFF processes these requests as displayed in the following diagram:

<p align="center">
    <img src="docs/diagrams/BFF-flow.png" width="300">
</p>

The BFF is also built to serve a website. This means the user must be able to navigate to it with a browser.

When the user navigates to the BFF, they can navigate to a special endpoint too: the /login endpoint. When the user navigates to this endpoint, the user is authenticated.

The process flow to authenticate a user is visualised in the following diagram:

<img src="docs/diagrams/BFF-login-workflow.png">

## Want to help making the internet more secure?

Get involved!

* Star this repository
* Follow this repository
* Create issues when for missing features
* Contribute to this repository
* Write articles about this repository