using Host.TestApps.TestApi;
using Microsoft.AspNetCore.Builder;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

[Binding]
public class EchoApiSteps(ScenarioContext scenarioContext)
{
    private WebApplication _app = null!;

    [BeforeStep]
    public async Task BeforeWhenStep()
    {
        var currentStepType = scenarioContext.StepContext.StepInfo.StepDefinitionType;
        if (currentStepType == StepDefinitionType.When)
        {
            var builder = WebApplication.CreateBuilder(Array.Empty<string>());

            _app = builder.Build();
        
            Program.MapEchoEndpoint(_app);
        
            _app.Urls.Add("http://localhost:8080");

            await _app.StartAsync();  
        }
    }
    
    [AfterScenario]
    public async Task TearDown()
    {
        await _app.StopAsync();
    }
}