using System.Net.Http.Headers;
using Bff;
using GoCloudNative.Bff.Authentication.Auth0;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

builder.Services.AddSecurityBff(o =>
{
    o.ConfigureAuth0(builder.Configuration.GetSection("Auth0"));
    o.LoadYarpFromConfig(builder.Configuration.GetSection("ReverseProxy"));
});

var app = builder.Build();

app.UseRouting();

app.UseSecurityBff();

// This is an example how you can execute multiple requests 
app.Map("/api/weatherforecast", async (HttpContext context, HttpClient httpClient) =>
{
    var accessToken = context.Session.GetAccessToken();
    if (string.IsNullOrEmpty(accessToken))
    {
        context.Response.StatusCode = 401;
        return;
    }

    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

    var apiBaseAddress = builder.Configuration
        .GetSection("ReverseProxy:Clusters:api:Destinations:api:Address")
        .Get<string>()
        .EnsureUrlEndsWithSlash();

    var usa = await httpClient.GetAsStringAsync($"{apiBaseAddress}api/weatherforecast/usa");
    var sahara = await httpClient.GetAsStringAsync($"{apiBaseAddress}api/weatherforecast/sahara");

    context.Response.ContentType = "application/json";
    await context.Response.WriteAsync("{ " +
                                        $"\"usa\": {usa}, " + 
                                        $"\"sahara\": {sahara} " +
                                      "}");
});

app.Run();