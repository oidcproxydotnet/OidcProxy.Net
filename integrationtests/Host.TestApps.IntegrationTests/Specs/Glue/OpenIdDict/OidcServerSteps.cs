using System.Security.Cryptography.X509Certificates;
using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using TechTalk.SpecFlow;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OpenIdDict;

[Binding]
public class OidcServerSteps(ScenarioContext scenarioContext)
{
    public static bool HasAuthenticated = false;
    
    private readonly OidcServerBuilder _builder = new ();
    
    private WebApplication? _app;

    [BeforeScenario]
    public void Reset()
    {
        HasAuthenticated = false;
    }

    [BeforeStep]
    public async Task BeforeStep()
    {
        if (_app == null && scenarioContext.RequiresBootstrap())
        {
            _app = _builder.Build();
            await _app.StartAsync();
        }
    }
    
    [Given(@"the Oidc Server encrypts JWEs with this symmetric key: (.*)")]
    public void SetSymmetricKey(string base64)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(base64));
        _builder.WithJweKey(key);
    }
    
    [Given(@"the Oidc server is configured to encrypt JWEs with this certificate: (.*), (.*)")]
    public void SetCertKey(string certPath, string keyPath)
    {
        var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        _builder.WithJweCert(cert);
    }

    [Then("the user has been authenticated")]
    public void AssertAuthenticated()
    {
        OidcServerSteps.HasAuthenticated.Should().BeTrue();
    }
    
    [Then("the user has not been authenticated")]
    public void AssertNotAuthenticated()
    {
        OidcServerSteps.HasAuthenticated.Should().BeFalse();
    }
    
    [Given(@"the Oidc Server signs the JWT with SHA256")]
    public void Void()
    {
        // this is the case by default
    }

    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}