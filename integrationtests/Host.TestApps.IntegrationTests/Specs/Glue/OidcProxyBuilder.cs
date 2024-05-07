using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

public class OidcProxyBuilder
{
    public string Url { get; private set; } = "https://localhost:8444";
    private bool _allowAnonymousAccess = true;
    private List<string> _whitelist = new();

    public OidcProxyBuilder WithUrl(string url)
    {
        Url = url;
        return this;
    }

    public OidcProxyBuilder AllowAnonymousAccess(bool allowAnonymousAccess)
    {
        _allowAnonymousAccess = allowAnonymousAccess;
        return this;
    }
    
    public OidcProxyBuilder WhitelistRedirectUrl(string url)
    {
        _whitelist.Add(url);
        return this;
    }
    
    public WebApplication Build()
    {
        OidcProxy.Net.ModuleInitializers.ModuleInitializer.Reset();
        
        var builder = WebApplication.CreateBuilder(Array.Empty<string>());
        
        var config = builder.Configuration
            .GetSection("OidcProxy")
            .Get<OidcProxyConfig>();

        config.AllowAnonymousAccess = _allowAnonymousAccess;

        if (_whitelist.Any())
        {
            config.AllowedLandingPages = _whitelist;
        }

        builder.Services.AddOidcProxy(config);

        var app = builder.Build();
        
        app
            .MapGet("/custom/me", async context => await context.Response.WriteAsJsonAsync(context.User.Identity?.Name))
            .RequireAuthorization();

        app
            .MapGet("/", () => "{}");
        
        app.UseOidcProxy();

        app.Urls.Add(Url);

        return app;
    }
}