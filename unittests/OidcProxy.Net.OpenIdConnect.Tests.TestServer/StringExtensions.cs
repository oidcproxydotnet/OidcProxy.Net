using Newtonsoft.Json.Linq;

namespace OidcProxy.Net.OpenIdConnect.Tests.TestServer;

public static class StringExtensions
{
    public static IResult ToOkResult(this string json)
    {
        return Results.Text(json);
    }
}