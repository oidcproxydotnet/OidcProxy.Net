using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
    
    public static void ConfigureDotnetDevCertExplicitlyIfItExists(this WebApplicationBuilder builder, string url)
    {
        var userProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        var dotnetToolsPath = Path.Combine(userProfile, ".dotnet", "tools", ".store", "dotnet-dev-certs");
        
        if (!Path.Exists(dotnetToolsPath))
        {
            return;
        }
        
        var certPath = Directory
            .GetFiles(dotnetToolsPath, "localhost.pfx", SearchOption.AllDirectories)
            .FirstOrDefault();

        if (!string.IsNullOrEmpty(certPath))
        {
            builder.WebHost.UseKestrel(options =>
            {
                var uri = new Uri(url);
                options.Listen(IPAddress.Any, uri.Port, o => o.UseHttps(certPath));
            });        
        }
    }
}