# Software architecture

The GoCloudNative.Bff.Authentication is a gateway. It interacts with the user, the identity provider, and downstream services. This is schematically displayed in the following diagram:

![Containers](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/containers.png)

De BFF acts as a reverse proxy. It augments forwarded requests by adding a Bearer token to the http request headers.

The GoCloudNative.Bff.Authentication is designed to be compatible with any OpenID Connect Server. It uses the OpenId Connect protocol to obtain `access_tokens`, `id_tokens`, and `refresh_tokens`.

To obtain these tokens, the components in the landscape interact as follows:

![Obtaining tokens](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/auth_code-sequence-diagram.png)

* The user navigates to the BFF, to the `/account/login` endpoint
* This endpoint redirects the user to the `/authorize` endpoint of the Identity Provider
* The Identity Provider serves a login page.
* The user logs in
* After the user has logged in successfully, the user is redirected back to the BFF, to the `/account/login/callback?code=xyz` endpoint
* The BFF reads the code from the query string and exchanges it for an `access_token`, an `id_token`, and a `refresh_token` by invoking the `/token` endpoint of the Identity Provider.
* The BFF stores this information in the HTTP-session

## The GoCloudNative.Bff.Authentication Modules
In essence, the GoCloudNative.Bff.Authentication as an aspnetcore site. The GoCloudNative.Bff.Authentication module adds authentication endpoints to it, it obtains tokens, augments http requests to downstream services, and it enables the HTTP session.

The GoCloudNative.Bff.Authentication provides modules to authenticate with common Identity Providers like IdentityServer4, Auth0, and Azure Active Directory. These are all implementations of the OpenId Connect protocol. Regardless, using the OpenId Connect, Auth0, nor the Azure AD module is required. You can implement your own identity provider.

Nontheless, most identity providers are OpenId Connect compatible. That's why there is a GoCloudNative.Bff.Authentication.OpenIdConnect module. Use this module to configure the BFF to use IdentityServer and KeyCloak, for example.

Unfortunately, not every IdentityProvider is 100% OpenId Connect compatible. That's why there is a GoCloudNative.Bff.Authentication.Auth0 and a GoCloudNative.Bff.Authentication.AzureAd module. They override the standard OpenId Connect behavior of the GoCloudNative.Bff.Authentication.OpenIdConnect module where Auth0 and Azure AD aren't compliant.

As a result, this is what the GoCloudNative.Bff.Authentication looks like from a module-perspective:

![Modules](https://raw.githubusercontent.com/thecloudnativewebapp/GoCloudNative.Bff/main/docs/gocloudnative.org/content/Diagrams/components.png)

### GoCloudNative.Bff.Authentication

This module implements OAuth2. In this module, the `/account` are defined. It provides an interface called `IIdentityProvider` which it uses to obtain the `/authorize` request, the token-request, the revoke-request, and the end-session request.

* __Forwarding requests to downstream services__\
The GoCloudNative.Bff.Authentication uses YARP to forward requests to downstream services. The GoCloudNative.Bff.Authentication registers an `ITransformProvider` to augment requests to the downstream services in `HttpHeaderTransformation.cs`. This is where the `Bearer` tokens are added to the requests.

* __BFF endpoints__\
The `/account`-endpoints are aspnetcore minimal apis. The solution is designed in such a way that it is possible to implement custom endpoints by simply adding ApiControllers or Minimal APIs to the host.

* __HttpSessions / scaling out__\
Tokens are stored in aspnetcore http sessions. They can be backed by a Redis Cache out of the box. This allows scaling out.

*  __Custom identity providers__\
The GoCloudNative.Bff.Authentication also provides a way to register custom implementations of the `IIdentityProvider` interface. This provides the flexibility to implement any OAuth compatible Identity Provider. Custom identity providers can be registered as follows:

```csharp
builder.Services.AddSecurityBff(o =>
{
    o.RegisterIdentityProvider<YourCustomIdentityProvider>();
}
```

This is the core of GoCloudNative.Bff.Authentication.

### GoCloudNative.Bff.Authentication.OpenIdConnect

The OpenIdConnect module is a facade for the `IdentityModel.OidcClient`. It generate all OpenId-Connect-requests.

Configure OpenId Connect as follows in the proram.cs:

```csharp
builder.Services.AddSecurityBff(o =>
{
    o.ConfigureOpenIdConnect(oidcConfig);
}
```

The module validates the configuration and throws errors if configured incorrectly.

### GoCloudNative.Bff.AuthenticationAzureAd
The AzureAd module derives from the OpenIdConnect module. It overrides the `OpenIdConnectIdentityProvider.GetAuthorizeUrlAsync` method and uses the `Microsoft.Identity.Client` nuget package to generate it instead.

Also, Azure Active Directory does not revoke `access_tokens`. That's why the `OpenIdConnectIdentityProvider.RevokeAsync` method has been intentionally left empty.

### GoCloudNative.Bff.Authentication.Auth0
The AzureAd module derives from the OpenIdConnect module. It overrides the `OpenIdConnectIdentityProvider.GetAuthorizeUrlAsync`. Unlike specificied in the [OpenId Connect protocol](https://openid.net/specs/openid-connect-core-1_0.html#AuthRequest), Auth0 requires the `audience` request parameter in the /authorize url. The Auth0 module adds it.

Also, Auth0 does not revoke `access_tokens`. That's why the `OpenIdConnectIdentityProvider.RevokeAsync` method has been intentionally left empty.

## The HOST project

The following modules are shipped in NuGet packages:

* GoCloudNative.Bff.Authentication
* GoCloudNative.Bff.Authentication.OpenIdConnect
* GoCloudNative.Bff.Authentication.Auth0
* GoCloudNative.Bff.Authentication.AzureAd

To be able to use it, they must be hosted in an aspnetcore project.