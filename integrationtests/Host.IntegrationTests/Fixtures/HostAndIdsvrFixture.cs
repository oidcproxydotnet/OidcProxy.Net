using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc.Razor.RuntimeCompilation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;

namespace Host.IntegrationTests.Fixtures;

public class HostAndIdsvrFixture : HostFixture
{
    private WebApplication? _app = null;
    
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());

        TestIdentityServer.Program.ConfigureServices(builder);

        var assembly = typeof(TestIdentityServer.Program).Assembly;
        builder.Services.AddControllersWithViews()
            .AddApplicationPart(assembly)
            .AddRazorRuntimeCompilation();

        builder.Services.Configure<MvcRazorRuntimeCompilationOptions>(options => 
            { options.FileProviders.Add(new EmbeddedFileProvider(assembly)); });

        _app = builder.Build();

        TestIdentityServer.Program.ConfigureApp(_app);

        _app.Urls.Add("https://localhost:7185");

        await _app.StartAsync();
       
    }

    public override async Task DisposeAsync()
    {
        await base.DisposeAsync();
        
        await _app.StopAsync();
    }
}