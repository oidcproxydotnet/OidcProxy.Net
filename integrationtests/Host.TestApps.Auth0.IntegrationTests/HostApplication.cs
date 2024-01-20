using OidcProxy.Net.Auth0;
using OidcProxy.Net.ModuleInitializers;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Host.TestApps.Auth0.IntegrationTests;

public class HostApplication : IAsyncLifetime, IDisposable
{
    private WebApplication? _testApi = null;
    private WebApplication? _echoApi = null;
    
    public virtual async Task InitializeAsync()
    {
        await StartOidcTestApiAsync();
        await StartEchoApiAsync();
    }

    private Task StartOidcTestApiAsync()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        
        // Add config
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false, true)
            .AddUserSecrets(typeof(Dummy).Assembly)
            .AddEnvironmentVariables()
            .Build();

        builder.Configuration.AddConfiguration(configuration);
        
        // Add proxy
        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<Auth0ProxyConfig>();

        builder.Services.AddAuth0Proxy(config);

        // Build and run..
        _testApi = builder.Build();

        _testApi.UseOidcProxy();

        _testApi.Urls.Add("https://localhost:8443");

        return _testApi.StartAsync();
    }
    
    private Task StartEchoApiAsync()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        _echoApi = builder.Build();
        
        TestApi.Program.MapEchoEndpoint(_echoApi);
        
        _echoApi.Urls.Add("http://localhost:8080");

        return _echoApi.StartAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await _testApi?.StopAsync()!;
        await _echoApi?.StopAsync()!;
    }

    public void Dispose() => DisposeAsync().GetAwaiter().GetResult();
}