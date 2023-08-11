using FluentAssertions;
using TheCloudNativeWebApp.Bff.Authentication.OpenIdConnect;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests;

public class OpenIdConnectConfigTests
{
    private readonly OpenIdConnectConfig _config = new()
    { 
        ClientId = "test",
        ClientSecret = "test",
        Authority = "https://authority.com"
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
    
    [Theory]
    [InlineData("htt://authority.com")]
    [InlineData("")]
    [InlineData(null)]
    public void WhenInvalidAuthority_ShouldThrowException(string invalid)
    {
        _config.Authority = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("authority", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
    }
    
    [Theory]
    [InlineData("https://foo.eu.auth0.com")]
    [InlineData("https://10.0.0.1")]
    [InlineData("https://127.0.0.1:5123")]
    [InlineData("http://10.0.0.1")]
    [InlineData("https://localhost")]
    [InlineData("https://localhost:8443")]
    public void WhenValidAuthority_ShouldNotThrowException(string valid)
    {
        _config.Authority = valid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeTrue();
        errors.Should().BeEmpty();
    }
}