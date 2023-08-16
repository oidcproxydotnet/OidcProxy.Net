using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Host.IntegrationTests.Fixtures;

public class HostApplication : IAsyncLifetime, IDisposable
{
    private WebApplication? _app = null;
    
    public virtual Task InitializeAsync()
    {
        var builder = WebApplication
            .CreateBuilder(Array.Empty<string>());
        
        var configurationBuilder = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets(typeof(Program).Assembly)
            .AddEnvironmentVariables();

        builder.Configuration.AddConfiguration(configurationBuilder.Build());

        var oidcConfig = Configuration.GetOpenIdConnectConfig();
        var auth0Config = Configuration.GetAuth0Config(builder.Configuration);
        
        Program.ConfigureBff(builder, oidcConfig, auth0Config, null);

        _app = builder.Build();

        Program.ConfigureApp(_app, builder);
        
        _app.Urls.Add("https://localhost:8443");

        return _app.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await _app?.StopAsync()!;
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();
}