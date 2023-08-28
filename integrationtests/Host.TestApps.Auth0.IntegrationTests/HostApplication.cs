using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
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
            .AddUserSecrets(typeof(HostApplication).Assembly)
            .AddEnvironmentVariables()
            .Build();

        builder.Configuration.AddConfiguration(configuration);
        
        // Add bff
        var config = builder.Configuration
            .GetSection("Bff")
            .Get<Auth0BffConfig>();

        builder.Services.AddBff(config);

        // Build and run..
        _testApi = builder.Build();

        _testApi.UseBff();

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