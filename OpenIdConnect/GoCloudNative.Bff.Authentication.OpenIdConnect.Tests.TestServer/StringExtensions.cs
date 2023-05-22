using Newtonsoft.Json.Linq;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests.TestServer;

public static class StringExtensions
{
    public static IResult ToOkResult(this string json)
    {
        return Results.Text(json);
    }
}