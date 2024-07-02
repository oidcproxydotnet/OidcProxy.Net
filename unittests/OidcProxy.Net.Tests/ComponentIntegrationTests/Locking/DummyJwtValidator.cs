using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Jwt.SignatureValidation;

namespace OidcProxy.Net.Tests.ComponentIntegrationTests.Locking;

public class DummyJwtValidator(IIdentityProvider identityProvider, Hs256SignatureValidator hs256SignatureValidator) 
    : JwtValidator(identityProvider, hs256SignatureValidator)
{
    public override Task<bool> Validate(string token)
    {
        return Task.FromResult(true);
    }
}