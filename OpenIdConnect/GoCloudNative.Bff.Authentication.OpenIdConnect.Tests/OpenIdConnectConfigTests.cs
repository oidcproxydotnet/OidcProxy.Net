using FluentAssertions;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests;

public class OpenIdConnectConfigTests
{
    public OpenIdConnectConfig _config = new()
    { 
        ClientId = "test"
    };

    [Fact]
    public void WhenValid_ShouldReturnTrue()
    {
        var actual = _config.Validate(out var errors);

        actual.Should().BeTrue();
        errors.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenInvalidClientId_ShouldThrowException(string invalid)
    {
        _config.ClientId = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("client_id", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
    }
}