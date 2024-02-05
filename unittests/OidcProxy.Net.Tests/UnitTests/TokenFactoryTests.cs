using FluentAssertions;
using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.Tests.UnitTests;

public class TokenFactoryTests
{
    [Theory]
    [InlineData("x.ewogICJzdWIiOiAiMjIzNDU2NzgiLAogICJuYW1lIjogIkphbmUgRG9lIiwKICAiaWF0IjogMTUxNjIzOTAyCn0.x")]
    [InlineData("x.ewogICJzdWIiOiAiMjIzNDU2NzEyIiwKICAibmFtZSI6ICJKb2huIERvZSIsCiAgImlhdCI6IDE1MTYyMzkwMjEKfQ.x")]
    public void WhenPayloadShouldContainPadding_ShouldDecodePayload(string token)
    {
        var sut = new JwtParser();
        
        var actual = sut.ParseAccessToken(token);

        actual.Sub.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("x.ewogICJzdWIiOiAiMTIzNDU2NzgiLAogICJuYW1lIjogIkpvaG4gRG9lIiwKICAiaWF0IjogMTUxNjIzOTAyMgp9.x")]
    public void WhenPayloadShouldNotContainPadding_ShouldDecodePayload(string token)
    {
        var sut = new JwtParser();
        
        var actual = sut.ParseAccessToken(token);

        actual.Sub.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void WhenNoPayload_ShouldReturnNull()
    {
        const string token = "x..y";
        var sut = new JwtParser();
        
        var actual = sut.ParseAccessToken(token);

        actual.Should().BeNull();
    }
}