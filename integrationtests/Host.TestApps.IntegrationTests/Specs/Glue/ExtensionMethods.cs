using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Bindings;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

public static class ExtensionMethods
{
    public static T Get<T>(this ScenarioContext ctx, string key) => (T)ctx[key];

    public static bool RequiresBootstrap(this ScenarioContext scenarioContext)
    {
        var currentStepType = scenarioContext.StepContext.StepInfo.StepDefinitionType;
        var currentStepText = scenarioContext.StepContext.StepInfo.Text;

        return (currentStepType == StepDefinitionType.When
                || currentStepText.Contains("the user has authenticated"));
    }
}