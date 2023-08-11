using System.Text;
using System.Text.RegularExpressions;
using FluentAssertions;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TokenFactoryTests
{
    [Theory]
    [InlineData("x.eydzdWInOiAnZTcxNjk1ODUtNDQ2YS00OTM4LWE3ODMtZTM0MDBiZTQwNmNiYzg4NWQzMGMtZmFjMi00NGZjLTlmMmEtMzMxOWYyZjNjM2Y5ZDA2NmM2MzQtY2RlZC00MDgzLWIyNDQtNmJlZDc1ZDAnIH0.x")]
    [InlineData("x.eydzdWInOiAnM2JkOGRkZWYtMDlmZS00YTc4LTk2ZjMtNDgxNWNlM2FhYWU1NWVkZmE5OTYtMjcyZi00MGZlLTg3MGQtM2U4MDUyYjNhNGQ4NzdlMDI3MGItMzc4Yi00MmQ0LTg0OTktZjQ0ODhlMScgfQ.x")]
    public void WhenPayloadShouldContainPadding_ShouldDecodePayload(string token)
    {
        var actual = token.ParseJwtPayload();

        actual.Sub.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("x.eydzdWInOiAnMmZiYTA4N2MtOTUxOS00MjBhLWEzMTItYjU1MWFjOTZmMGI4M2FiNGY0YTItN2VmZS00Y2M1LTgxNTUtZjkyMzQwZTU5YTA2ODFjMWYwNmUtMjM0MS00OWRiLTlmY2EtZjJlOTY2JyB9.x")]
    public void WhenPayloadShouldNotContainPadding_ShouldDecodePayload(string token)
    {
        var actual = token.ParseJwtPayload();

        actual.Sub.Should().NotBeNullOrEmpty();
    }

}