using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Host.TestApps.Oidc.IntegrationTests;

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

        var config = builder.Configuration
            .GetSection("Bff")
            .Get<OidcBffConfig>();
        
        builder.Services.AddBff(config);

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