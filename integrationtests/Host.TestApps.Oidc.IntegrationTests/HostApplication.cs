using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
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
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();
        
        builder.Services.AddOidcProxy(config);

        _testApi = builder.Build();
        
        _testApi
            .MapGet("/custom/me", async context => await context.Response.WriteAsJsonAsync(context.User.Identity?.Name))
            .RequireAuthorization();
        
        _testApi.UseOidcProxy();

        _testApi.Urls.Add("https://localhost:8444");

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