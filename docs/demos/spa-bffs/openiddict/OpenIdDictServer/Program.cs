using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        options
            .AllowAuthorizationCodeFlow()
            .AllowRefreshTokenFlow();

        // Done!
        // options.AddEncryptionKey(new SymmetricSecurityKey(
        //     Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")));

        // Done!
        options.AddEncryptionCertificate(X509Certificate2.CreateFromPemFile("cert.pem", "key.pem"));

        // options.AddEncryptionCredentials(new EncryptingCredentials(
        //     new SymmetricSecurityKey(Convert.FromBase64String("DRjd/GnduI3Efzen9V9BvbNUfc/VKgXltV7Kbk9sMkY=")),
        //     SecurityAlgorithms.HmacSha512
        // ));

        options
            .AddDevelopmentSigningCertificate();

        options
            .UseAspNetCore()
            .EnableTokenEndpointPassthrough()
            .EnableAuthorizationEndpointPassthrough();
    });

    // Register the OpenIddict validation components.
    // .AddValidation(options =>
    // {
    //     // Import the configuration from the local OpenIddict server instance.
    //     options.UseLocalServer();
    //
    //     // Register the ASP.NET Core host.
    //     options.UseAspNetCore();
    // });

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
