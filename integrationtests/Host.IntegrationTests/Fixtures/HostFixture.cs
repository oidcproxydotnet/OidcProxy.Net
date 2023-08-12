using Microsoft.AspNetCore.Builder;
using Xunit;

namespace Host.IntegrationTests.Fixtures;

public class HostFixture : IAsyncLifetime, IDisposable
{
    private WebApplication? _app = null;

    public virtual Task InitializeAsync()
    {
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        var oidcConfig = Configuration.GetOpenIdConnectConfig();
        
        Program.ConfigureBff(builder, oidcConfig, null, null);

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