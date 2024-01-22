using System.Net.Http.Headers;
using Bff;
using OidcProxy.Net;
using OidcProxy.Net.OpenIdConnect;
using OidcProxy.Net.ModuleInitializers;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpClient();

var config = builder.Configuration
    .GetSection("Bff")
    .Get<OidcProxyConfig>();

builder.Services.AddOidcProxy(config);

var app = builder.Build();

app.UseRouting();

app.UseOidcProxy();

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

    var usa = await httpClient.GetAsStringAsync("http://localhost:8080/api/weatherforecast/usa");
    var sahara = await httpClient.GetAsStringAsync("http://localhost:8080/api/weatherforecast/sahara");

    await context.Response.WriteAsJsonAsync("{ " +
                                            $"\"usa\": { usa }, " +
                                            $"\"sahara\": {sahara} " +
                                            "}");
});

app.Run();