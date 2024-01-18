using FluentAssertions;
using GoCloudNative.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.Tests.UnitTests;

public class TokenFactoryTests
{
    [Theory]
    [InlineData("x.ewogICJzdWIiOiAiMjIzNDU2NzgiLAogICJuYW1lIjogIkphbmUgRG9lIiwKICAiaWF0IjogMTUxNjIzOTAyCn0.x")]
    [InlineData("x.ewogICJzdWIiOiAiMjIzNDU2NzEyIiwKICAibmFtZSI6ICJKb2huIERvZSIsCiAgImlhdCI6IDE1MTYyMzkwMjEKfQ.x")]
    public void WhenPayloadShouldContainPadding_ShouldDecodePayload(string token)
    {
        var actual = token.ParseJwtPayload();

        actual.Sub.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("x.ewogICJzdWIiOiAiMTIzNDU2NzgiLAogICJuYW1lIjogIkpvaG4gRG9lIiwKICAiaWF0IjogMTUxNjIzOTAyMgp9.x")]
    public void WhenPayloadShouldNotContainPadding_ShouldDecodePayload(string token)
    {
        var actual = token.ParseJwtPayload();

        actual.Sub.Should().NotBeNullOrEmpty();
    }

}