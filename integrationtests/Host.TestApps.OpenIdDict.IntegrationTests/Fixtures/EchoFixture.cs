using Host.TestApps.TestApi;
using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Host.TestApps.OpenIdDict.IntegrationTests.Fixtures;

public class EchoFixture : IAsyncLifetime, IDisposable
{
    private WebApplication? _echoApi = null;

    public Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        _echoApi = builder.Build();
        
        Program.MapEchoEndpoint(_echoApi);
        
        _echoApi.Urls.Add("http://localhost:8080");

        return _echoApi.StartAsync();    
    }

    public async Task DisposeAsync()
    {
        await _echoApi?.StopAsync()!;
    }

    public void Dispose()
    {
        DisposeAsync().GetAwaiter().GetResult();
    }
}