using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.DependencyInjection;

namespace Host.TestApps.OpenIdDict.IntegrationTests.Fixtures.OpenIddict;

public class OpenIddictCertFixture : OpenIddictFixture
{
    protected override void AddEncryptionMethod(OpenIddictServerBuilder options)
    {
        var cert = X509Certificate2.CreateFromPemFile("cert.pem", "key.pem");
        options.AddEncryptionCertificate(cert);
    }
}