using System.Security.Claims;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("Bff")
    .Get<OidcBffConfig>();

builder.Services.AddBff(config);

var app = builder.Build();

app.MapGet("/custom/me", async context =>
    {
        var identity = (ClaimsIdentity)context.User.Identity;
        await context.Response.WriteAsJsonAsync(new
        {
            Sub = identity.Name,
            Claims = identity.Claims.Select(x => new
            {
                Type = x.Type,
                Value = x.Value
            })
        });
    })
    .RequireAuthorization();

app.UseBff();

app.Run();
