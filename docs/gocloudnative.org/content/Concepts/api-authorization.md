---
author: Albert Starreveld
title: API Authorization with OAuth2/OpenId Connect
description: Read how to implement OAuth2/OIDC in a aspnetcore C# application.
tags: ["OAuth2", "API", "aspnetcore", "OIDC", "Authorization"]
---
# Implementing API Authorization with OAuth2 and OpenId Connect (OIDC)

OAuth2 and OpenID Connect (OIDC) protocols are robust and reliable solutions for establishing secure access control and verifying identities. While OAuth2 focuses on authorization, granting or denying access to valuable resources, OpenID Connect extends OAuth2 by providing an identity protocol that emits the identity of individuals or machines.

One notable advantage of both OAuth2 and OIDC is their decentralized authentication approach. This decentralized authentication enables seamless scalability within application landscapes utilizing OAuth2 or OIDC servers.

When implementing authorization in an API, it is advisable to follow these key concepts to leverage the benefits of decentralized authentication:

* Utilize the OIDC-server for efficient and reliable authentication purposes.
Ensure the OIDC-server remains application agnostic, promoting flexibility and adaptability.
* Apply well-defined policies to enforce robust authorization rules.
* Safeguard sensitive data by refraining from storing Personally Identifiable Information (PII) within your APIs.
* By adhering to these principles, you can establish a highly secure and scalable API ecosystem empowered by OAuth2 and OIDC.

## Leveraging the OpenID Connect Protocol for enhanced Authentication and Authorization

The OpenID Connect protocol offers effective solutions to address various authentication and authorization challenges. To fully benefit from its capabilities, consider the following aspects when implementing authorization in your API:

### Identity and Access Management

When implementing OIDC-based authentication in your API, you delegate user authentication to another application, simplifying the process. However, there are inherent risks in this delegation. The more information the OIDC server has about other applications, the more complex releasing them becomes. Additionally, as new applications emerge, the OIDC server's data storage requirements increase, leading to scalability issues.

To optimize scalability, follow a rule of thumb: minimize the OIDC server's knowledge of other applications. Focus on common OIDC claims to enhance efficiency and simplify integration.

_Example: Common scenario's to avoid:_

* _Implementing application-specific roles like "crm_admin"_
* _Creating application-specific claims like "is_crm_admin"_
* _And so forth_

_Examples: Instead do this:_
* _role: "hr_secretary", applications may decide what authorization applies for users in such a role._

### Streamline Identity Exchange and Ensure GDPR Compliance with OIDC

In an OIDC server, the exchange of identity information between applications occurs seamlessly through `id_tokens`. These tokens encompass Personal Identifiable Information (PII). By leveraging these tokens appropriately, you can optimize data handling while maintaining GDPR compliance.

When the front-end utilizes the `id_token` solely for displaying user names and the back-end exclusively relies on roles, user IDs, and scopes, there is no necessity for the API to store any Personal Identifiable Information. Consequently, all PII remains centralized within the OIDC server. This centralized approach simplifies GDPR compliance efforts and ensures the convenience of managing sensitive data in one secure location.

## How to implement authorization in your API
So, to implement authorization in a scalable, safe way, make sure to do the following:

* Do not include the `id_token` in API requests
* Include the `access_token` in the request to an API
* Use the information in the `access_token` to apply a `policy`.
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

## Enhance Authorization with Claims Transformation

In certain cases, the information contained within the access token may not provide sufficient details to determine authorization for resource access. To address this, it becomes necessary to enrich the context by combining the information from the access token with application-specific data, enabling the application to apply custom policies.

This process of enriching the context is known as claims transformation. To implement claims transformation effectively, consider utilizing the IClaimsTransformation interface, which allows you to seamlessly integrate and customize this functionality within your application.

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

To apply a custom policy on this claim, implement policy-based authorization by adding the following code snippet to `program.cs`:

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

As an alternative, you may consider implementing [Open Policy Agent](https://www.openpolicyagent.org/).

## Summary

Consider the following key concepts when implementing API authorization with OAuth2 and OIDC:

* Utilize the OIDC-server for _authentication_.
* Build _policies_ in the APIs to determine a users' permissions
* Add more context if need be, by utilizing a concept called `claims transformation`
* Refraining from storing Personally Identifiable Information (PII) in APIs.

To ensure maintainability of the application landscape, it is important to keep the OIDC server application agnostic. This can be achieved by adhering to common OIDC claims whenever possible.


## Sources

* https://abstarreveld.medium.com/claims-transformation-in-net-6-483c30705e12
* https://learn.microsoft.com/en-us/aspnet/core/security/authentication/claims?view=aspnetcore-6.0#extend-or-add-custom-claims-using-iclaimstransformationcore-6.0
* https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies?view=aspnetcore-7.0