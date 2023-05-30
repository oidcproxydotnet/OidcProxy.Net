using FluentAssertions;

namespace GoCloudNative.Bff.Authentication.Auth0.Tests;

public class Auth0ConfigTests
{
    private readonly Auth0Config _config = new()
    { 
        ClientId = "test",
        ClientSecret = "test",
        Domain = "yourdomain.eu.auth0.com",
        Audience = "https://your/api/audience"
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
    [InlineData("https://authority")]
    [InlineData("htt://authority.com")]
    [InlineData("foo.eu.auth0.com/foo")]
    [InlineData("")]
    [InlineData(null)]
    public void WhenInvalidDomain_ShouldThrowException(string invalid)
    {
        _config.Domain = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("domain", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
    }
    
    [Theory]
    [InlineData("foo.eu.auth0.com")]
    public void WhenValidDomain_ShouldNotThrowException(string valid)
    {
        _config.Domain = valid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeTrue();
        errors.Should().BeEmpty();
    }
    
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenInvalidAudience_ShouldThrowException(string invalid)
    {
        _config.Audience = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("audience", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
    }
}