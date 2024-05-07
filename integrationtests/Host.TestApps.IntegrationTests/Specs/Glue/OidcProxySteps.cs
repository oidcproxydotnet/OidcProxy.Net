using Microsoft.AspNetCore.Builder;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

[Binding]
public class OidcProxySteps(ScenarioContext scenarioContext)
{
    private readonly OidcProxyBuilder _builder = new ();
    
    private WebApplication _app;

    [BeforeStep]
    public async Task BeforeWhenStep()
    {
        var currentStepType = scenarioContext.StepContext.StepInfo.StepDefinitionType;
        if (currentStepType == StepDefinitionType.When)
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

    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}