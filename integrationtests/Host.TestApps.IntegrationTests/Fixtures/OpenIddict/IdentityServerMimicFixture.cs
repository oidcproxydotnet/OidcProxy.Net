using Microsoft.Extensions.DependencyInjection;

namespace Host.TestApps.IntegrationTests.Fixtures.OpenIddict;

public class IdentityServerMimicFixture : OpenIddictFixture
{
    protected override void AddEncryptionMethod(OpenIddictServerBuilder options)
    {
        options.DisableAccessTokenEncryption();

        options.AddDevelopmentEncryptionCertificate();
    }
}