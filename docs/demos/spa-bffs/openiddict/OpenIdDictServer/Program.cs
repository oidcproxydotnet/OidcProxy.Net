using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpenIdDictServer;
using OpenIdDictServer.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors();
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlite($"Filename={Path.Combine(Path.GetTempPath(), "openiddict-oidcproxy-test-server.sqlite3")}");
    options.UseOpenIddict();
});

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddOpenIddict()
    .AddCore(o => o.UseEntityFrameworkCore().UseDbContext<ApplicationDbContext>())
    .AddServer(options =>
    {
        options.SetTokenEndpointUris("connect/token");

        options.SetAuthorizationEndpointUris("connect/authorize");
        
        //options.DisableAccessTokenEncryption();

        options
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        // <don't use this in production!>
        options
            .AddDevelopmentEncryptionCertificate()
            .AddDevelopmentSigningCertificate();
        // </don't use this in production!>

        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough();
    });

builder.Services.AddHostedService<Worker>();

var app = builder.Build();

app.UseDeveloperExceptionPage();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(options =>
{
    options.MapControllers();
    options.MapDefaultControllerRoute();
});

app.Run();
