using System.Net.Http.Headers;
using OidcProxy.Net.EntraId;
using Bff;
using OidcProxy.Net;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration
    .GetSection("OidcProxy")
    .Get<EntraIdProxyConfig>();

builder.Services.AddEntraIdProxy(config);

var app = builder.Build();

app.UseRouting();

app.UseEntraIdProxy();

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