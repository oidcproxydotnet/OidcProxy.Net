using OidcProxy.Net.Jwt.SignatureValidation;

namespace OidcProxy.Net.Tests.ComponentIntegrationTests.Locking;

public class DummyJwtSignatureValidator : IJwtSignatureValidator
{
    public Task<bool> Validate(string? token)
    {
        return Task.FromResult(true);
    }
}