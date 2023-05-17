using GoCloudNative.Bff.Authentication.OpenIdConnect.Tests.TestServer;

TestProgram.Requests.Clear();

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Map("/.well-known/openid-configuration", () =>
{
    TestProgram.Requests.Add(new Request("/.well-known/openid-configuration"));
    return File.ReadAllText("Content/WellKnown.json").ToOkResult();
});

app.MapPost("/oauth/token", () =>
{
    TestProgram.Requests.Add(new Request("/oauth/token"));
    return TestProgram.AccessTokenResponse.ToOkResult();
});

app.MapPost("/oauth/revoke", () =>
{
    TestProgram.Requests.Add(new Request("/oauth/revoke"));
    return File.ReadAllText("Content/RevokeResponse.json").ToOkResult();
});

app.Run();

public partial class TestProgram
{
    public static List<Request> Requests = new();

    public static string AccessTokenResponse = File.ReadAllText("Content/AccessTokenResponse.json");
}