# The /account/me endpoint

When a user is authenticated on the client-side, the Single-Page Application receives an `id_token`. This token contains various user information, such as the username.

However, when authentication is shifted to the server-side, the front-end no longer receives this token. This poses a problem because it hinders the ability to display the username or generate a menu based on user permissions.

To address this issue, the GoCloudNative.Bff provides a `/account/me` endpoint. By making a GET request to this endpoint, you can view the contents of the `id_token` payload.

This is what a typical response of the `/account/me` endpoint looks like:

```json
{
  "ver": "2.0",
  "iss": "https://login.microsoftonline.com/9122040d-6c67-4c5b-b112-36a304b66dad/v2.0",
  "sub": "AAAAAAAAAAAAAAAAAAAAAIkzqFVrSaSaFHy782bbtaQ",
  "aud": "6cb04018-a3f5-46a7-b995-940c78f5aef3",
  "exp": 1536361411,
  "iat": 1536274711,
  "nbf": 1536274711,
  "name": "Abe Lincoln",
  "preferred_username": "AbeLi@microsoft.com",
  "oid": "00000000-0000-0000-66f3-3332eca7ea81",
  "tid": "9122040d-6c67-4c5b-b112-36a304b66dad",
  "nonce": "123523",
  "aio": "Df2UVXL1ix!lMCWMSOJBcFatzcGfvFGhjKv8q5g0x732dR5MB5BisvGQO7YWByjd8iQDLq!eGbIDakyp5mnOrcdqHeYSnltepQmRp6AIZ8jY"
}
```

## Customization

The specific claims present in the `id_token` are configured within the Identity Provider. In theory, this token may be exposed to the client since it is not used for authentication purposes. However, there may be instances where it is undesirable to display all the claims from the `id_token` in the `/account/me` endpoint.

Additionally, there may be cases where the claims within the `id_token` are insufficient for building the required front-end functionality.

To overcome these challenges, the `/account/me` endpoint offers customization options to tailor its behavior according to specific needs.

### Customizing the contents of the `/account/me` endpoint

The `/account/me` endpoint, by default, performs the decoding and parsing of the `id_token` payload. It utilizes the `JwtPayload.Parse` method from the `System.IdentityModel.Tokens.Jwt` package to accomplish this. The resulting parsed payload is then returned by the `/account/me` endpoint in JSON format.

All of this functionality is implemented within the `GoCloudNative.Bff.Authentication` package. Within this package, there exists an interface named `IClaimsTransformation` within the `GoCloudNative.Bff.Authentication.OpenIdConnect` namespace.

Here is the signature of the `IClaimsTransformation` interface:

```csharp
public interface IClaimsTransformation
{
    Task<object> Transform(JwtPayload payload);
}
```

By default, the GoCloudNative.Bff uses this implementation:

```csharp
internal class DefaultClaimsTransformation : IClaimsTransformation
{ 
    public Task<Object> Transform(JwtPayload payload) 
    {
        return Task.FromResult<object>(payload);
    }
}
```

You may choose to build your own implementation. Like so:

```csharp
public class MyClaimsTransformation : IClaimsTransformation
{
    public Task<object> Transform(JwtPayload payload)
    {
        var result = new
        {
            Sub = payload.Sub,
            Name = payload.Claims.Where(x => x.Type == "name").Select(x => x.Value).FirstOrDefault()
        };

        return Task.FromResult<object>(result);
    }
}
```

In this example, the only thing that is displayed in the `/account/me` endpoint is the user id and the username.

To register this class, use the following method:

```csharp

builder.Services.AddSecurityBff(o =>
{
// ...
    o.AddClaimsTransformation<MyClaimsTransformation>();
// ...
});
```

By registering the class in this manner, it is registered as a service in the servicecollection. As a result, it becomes possible to utilize dependency injection just as you are accustomed to: you can specify dependencies in the constructor as you are familiar with.

### Customizing the endpoint address

By default, the `/me` endpoint is registered at `/account/me`. However, this can also be customized.

During the registration of the Identity Provider, you have the option to specify a second parameter, as demonstrated below:

```csharp
builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(oidcConfig, "auth");
});
```

Or

```csharp
builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(auth0Config, "auth");
});
```

Or

```csharp
builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAzureAd(aadConfig, "auth");
});
```

By utilizing this code, the `/account` endpoints will be replaced and no longer accessible. Instead, you should now utilize the `/auth` endpoints. Consequently, the login endpoint has been relocated to `/auth/login`, the me endpoint has been relocated to `/auth/me`, and the end-session endpoint has been relocated to `/auth/end-session`. You may choose any value for this parameter.