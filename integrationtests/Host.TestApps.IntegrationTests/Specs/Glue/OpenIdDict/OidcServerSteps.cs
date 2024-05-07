using FluentAssertions;
using Microsoft.AspNetCore.Builder;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Host.TestApps.IntegrationTests.Specs.Glue.OpenIdDict;

[Binding]
public class OidcServerSteps(ScenarioContext scenarioContext)
{
    public static bool HasAuthenticated = false;
    
    private readonly OidcServerBuilder _builder = new ();
    
    private WebApplication _app;

    [BeforeScenario]
    public void Reset()
    {
        HasAuthenticated = false;
    }

    [BeforeStep]
    public async Task BeforeWhenStep()
    {
        var currentStepType = scenarioContext.StepContext.StepInfo.StepDefinitionType;
        if (currentStepType == StepDefinitionType.When)
        {
            _app = _builder.Build();
            await _app.StartAsync();
        }
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

    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}