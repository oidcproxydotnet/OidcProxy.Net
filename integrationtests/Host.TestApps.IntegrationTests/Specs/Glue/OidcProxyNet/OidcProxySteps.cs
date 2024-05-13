using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Unicode;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.OpenIdConnect.Jwe;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OidcProxyNet;

[Binding]
public class OidcProxySteps(ScenarioContext scenarioContext)
{
    private readonly OidcProxyBuilder _builder = new ();
    
    private WebApplication? _app;

    [BeforeStep]
    public async Task BeforeStep()
    {
        if (_app == null && scenarioContext.RequiresBootstrap())
        {
            _app = _builder.Build();
            await _app.StartAsync();

            scenarioContext["proxyurl"] = _builder.Url;
        }
    }

    [Given("the OidcProxy is configured to disallow anonymous access")]
    public void DisallowAnonymousAccess()
    {
        _builder.AllowAnonymousAccess(false);
    }
    
    [Given("the OidcProxy is configured to allow anonymous access")]
    public void AllowAnonymousAccess()
    {
        _builder.AllowAnonymousAccess(true);
    }

    [Given("the OidcProxy has included (.*) in the whitelisted redirect urls")]
    public void Whitelist(string url)
    {
        _builder.WhitelistRedirectUrl(url);
    }
    
    [Given(@"the user of OidcProxy has implemented claims transformation")]
    public void AddClaimsTransformation()
    {
        _builder.WithClaimsTransformation();
    }

    [Given(@"the OidcProxy is configured to decrypt JWEs with this symmetric key: (.*)")]
    public void SetSymmetricKey(string base64)
    {
        var key = new SymmetricSecurityKey(Convert.FromBase64String(base64));
        _builder.WithJweKey(new EncryptionKey(key));
    }

    [Given(@"the OidcProxy is configured to decrypt JWEs with this certificate: (.*), (.*)")]
    public void SetCertKey(string certPath, string keyPath)
    {
        var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        _builder.WithJweCert(cert);
    }

    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}