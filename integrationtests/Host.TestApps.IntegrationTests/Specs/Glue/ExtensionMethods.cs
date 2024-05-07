using TechTalk.SpecFlow;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

public static class ExtensionMethods
{
    public static T Get<T>(this ScenarioContext ctx, string key) => (T)ctx[key];
}