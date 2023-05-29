using FluentAssertions;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests;

public class OpenIdConnectConfigTests
{
    private readonly OpenIdConnectConfig _config = new()
    { 
        ClientId = "test",
        ClientSecret = "test"
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
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenInvalidClientSecret_ShouldThrowException(string invalid)
    {
        _config.ClientSecret = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("client_secret", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
    }
}