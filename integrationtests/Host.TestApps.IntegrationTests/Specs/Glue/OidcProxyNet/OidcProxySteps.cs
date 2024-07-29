using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using OidcProxy.Net.Cryptography;
using TechTalk.SpecFlow;

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
        _builder.WithEncryptionKey(new SymmetricKey(key));
    }

    [Given(@"the OidcProxy is configured to decrypt JWEs with this certificate: (.*), (.*)")]
    public void SetCertKey(string certPath, string keyPath)
    {
        var cert = X509Certificate2.CreateFromPemFile(certPath, keyPath);
        _builder.WithJweCert(cert);
    }
    
    [Given(@"the OidcProxy has been configured to use an ASP.NET Core Policy")]
    public void ConfigurePolicy()
    {
        _builder.WithPolicy();
    }

    [Given(@"the OidcProxy is configured to use a symmetric key to validate the token signature")]
    public void SetSigningKey()
    {
        _builder.WithSigningKey();
    }

    [Given(@"the Proxy receives a token that has been tampered with")]
    public void MymmicMitmAttack()
    {
        _builder.WithMitm(AbuseCase.TamperedPayload);
    }

    [Given(@"the Proxy receives a token with a (.*)")]
    public void MimmicMitmAttack(string abuseCase)
    {
        switch (abuseCase)
        {
            case "payload that has been tampered with":
                _builder.WithMitm(AbuseCase.TamperedPayload);
                break;
            case "payload and header that has been tampered with":
                _builder.WithMitm(AbuseCase.TamperedPayload | AbuseCase.ChangedAlgorithm);
                break;
            case "payload that has been tampered with and no JWT header":
                _builder.WithMitm(AbuseCase.TamperedPayload | AbuseCase.RemovedHeader);
                break;
            case "payload that has been tampered with and two trailing JWT sections":
                _builder.WithMitm(AbuseCase.TamperedPayload | AbuseCase.TrailingDots);
                break;
            default:
                throw new PendingStepException();
        }
    }

    [Given(@"the user's access_token has expired")]
    public void MymmicTokenExpiry()
    {
        _builder.WithExpiredAccessToken();
    }
    
    [Given(@"the proxy runs in AuthenticateOnly-Mode")]
    public void RemoveYarpFromConfigAndSetToAuthenticationOnlyMode()
    {
        _builder.WithAuthenticateOnlyMode();
    }
    
    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}