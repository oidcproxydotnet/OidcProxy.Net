using TestIdentityServer.ModuleInitializers;
using Microsoft.AspNetCore.HttpOverrides;

namespace TestIdentityServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        ConfigureServices(builder);

        var app = builder.Build();

        ConfigureApp(app);

        app.Run();
    }

    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddControllersWithViews();
        builder.Services.AddIdentityServer4();
    }
    
    public static void ConfigureApp(WebApplication app)
    {
        app.UseForwardedHeaders(new ForwardedHeadersOptions
        {
            ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
        });

        app.UseCertificateForwarding();
        app.UseCookiePolicy();

        app.UseDeveloperExceptionPage();
        app.UseStaticFiles();

        app.UseRouting();
        app.UseIdentityServer();

        app.UseAuthorization();

        app.MapDefaultControllerRoute();
    }
}