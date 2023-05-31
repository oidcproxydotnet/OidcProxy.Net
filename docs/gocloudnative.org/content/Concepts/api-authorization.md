---
author: Albert Starreveld
title: API Authorization with OAuth2/OpenId Connect
description: Read how to implement OAuth2/OIDC in a aspnetcore C# application.
tags: ["OAuth", "OAuth2", "C#", "API", "aspnetcore", "OpenId Connect", "access_tokens"]
---
# API Authorization with OAuth2/OpenId Connect

OAuth2 is an authorization protocol. It is meant to be used to grant or deny access to resources. OpenId Connect extends the OAuth2 protocol. OpenId Connect (OIDC) is an identity protocol. It emits the identity of either a person or a machine.

An important aspect of both OAuth2 and OIDC is that both protocols authenticate a person decentrally. This allows the application landscape which uses an OAuth2- or an OIDC-server to scale. 

To get the benefits that go with decentralized authentication, there are certain concepts to keep in mind when you're implementing authorization in an API:

1. Use the OIDC-server for authentication purposes
2. Make sure the OIDC-server is application agnostic
3. Apply policies
4. Do not store Personal Identifyable Information in your APIs

## Implications

The OpenId Connect protocol is built in such a way that it solves a number of common authentication- and authorization problems. If you want to profit from this, keep in mind the following things when you implement authorization in your API:

### Identity and Access Management

Implementing authorization in an API is means to an end. When you are implementing OIDC-based authentication, you're delegating the responsibility to authenticate users to another application.

Delegating this responsiblity comes with a risk. The more the OIDC-server "knows" about another application, the harder it gets to release the two. Also, when more application's are 'born', the OIDC-server will have to store more and more data. This does not scale well. 

Therefor, as a common rule of thumb, try to reduce what the OIDC-server knows about other applications to a minimum. Stick to common OIDC-claims as much as you can.

_Example: Common scenario's to avoid:_

* _Implementing application-specific roles like "crm_admin"_
* _Creating application-specific claims like "is_crm_admin"_
* _And so forth_

_Examples: Instead do this:_
* _role: "hr_secretary", applications may decide what authorization applies for users in such a role._

### PII
An OIDC server exchanges identity-information with an application through access- and id-tokens. These tokens contain Personal Identifyable Information. When the front-end uses the id_token to display a user's name, for example, and when the back-end uses nothing but roles, user-ids, and scopes, then there is no need for an API to store Personal Identifyable Information. 

As a result, Personal Identifyable Information is always in one place: The OIDC Server. This is very convenient when you want to be GDPR-compliant.

## How to implement authorization in your API
So, to implement authorization in a scalable, safe way, make sure to do the following:

* Do not include the `id_token` in API requests
* Include the `access_token` in the request to an API
* Use the information int he `access_token` to apply a `policy`.
* Do not build your own authorization middleware, instead use `Microsoft.AspNetCore.Authentication`.

Given an API does not have a user interface, it does not care how the consumer of the API obtained an `access_token`. All it should care about is if it is a valid token. So, in your `program.cs`, implement the following code:

.NET 6:

```csharp
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ...

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Bearer", options =>
    {
        options.Authority = "https://login.yourdomain.com";

        options.TokenValidationParameters = new TokenValidationParameters
        {
            //..
        };
    });

// ...

var app = builder.Build();

// ...

app.UseAuthorization();

// ...

app.Run();
```

And decorate the endpoints in your controllers as such:

.NET 6:

```csharp
[ApiController]
[Route("api/test")]
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    [Authorize] // Use this attribute to enforce authentication
    public IActionResult Get()
    {
        var user = this.User.Identity as ClaimsIdentity;
        return Ok($"Hello, {user.Name}!");
    }
}
```

Find a sample implementation [here](https://github.com/thecloudnativewebapp/GoCloudNative.Bff/blob/main/docs/Integration-Manuals/Integrating-With-Identity-Providers/IdentityServer4/src/Api/Program.cs).

## Advanced authorization scenario's

Sometimes, the information stored in the `access_token` is not sufficient to determine whether someone is authorized to access a resource or not. In that case, you need to enritch the context, so you can apply a policy on the information from the `access_token` combined with application specific information. 

Enritching the context as such is called claims-transformation. This can be applied by implementing the `IClaimsTransformation` interface:

```csharp
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;

public class MyClaimsTransformation : IClaimsTransformation
{
    public Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        ClaimsIdentity claimsIdentity = new ClaimsIdentity();
        var claimType = "myNewClaim";
        if (!principal.HasClaim(claim => claim.Type == claimType))
        {
            claimsIdentity.AddClaim(new Claim(claimType, "myClaimValue"));
        }

        principal.AddIdentity(claimsIdentity);
        return Task.FromResult(principal);
    }
}
```

Register the middleware in `program.cs` as follows:

```csharp
builder.Services.AddTransient<IClaimsTransformation, MyClaimsTransformation>();
```

To apply a custom policy on this claim, implement policy-based authorization by adding the following code-snippet to `program.cs`:

```csharp
builder.Services.AddAuthorization(o =>
{
    o.AddPolicy("onlyMyNewClaim", p => p.RequireClaim("myNewClaim", "myClaimValue"));
});
```

And apply the policy in your controller:
```csharp
[HttpGet]
[Authorize("onlyMyNewClaim")]
public IActionResult Get()
{
    return Ok("I am authorized!");
}
```

### Even more advanced scenario's

As an alternative you may consider implementing [Open Policy Agent](https://www.openpolicyagent.org/).

## Summary
Use OpenId for authentication-purposes. Apply a policy in each individual API to determine whether or not the consumer of the API is authorized.

## Sources

* https://abstarreveld.medium.com/claims-transformation-in-net-6-483c30705e12
* https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims?view=aspnetcore-6.0#extend-or-add-custom-claims-using-iclaimstransformationcore-6.0
* https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-7.0