using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

internal interface IOidcProxyBootstrap : IBootstrap
{
    IOidcProxyBootstrap WithCallbackHandler<TCallbackHandler>()
        where TCallbackHandler : IAuthenticationCallbackHandler;

    IOidcProxyBootstrap WithClaimsTransformation<TClaimsTransformation>()
        where TClaimsTransformation : IClaimsTransformation;

    IOidcProxyBootstrap WithRedirectUriFactory<TRedirectUriFactory>()
        where TRedirectUriFactory : IRedirectUriFactory;
}