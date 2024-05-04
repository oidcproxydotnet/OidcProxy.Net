using FluentAssertions;
using OidcProxy.Net.EntraId;

namespace EntraId.Tests;

public class AzureAdConfigTests
{
    private readonly EntraIdConfig _config = new()
    { 
        ClientId = "test",
        ClientSecret = "test",
        Authority = "https://authority.com",
        TenantId = "12345-adsgafh"
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
        errors.Any(x => x.Contains("-AZ-", StringComparison.InvariantCulture)).Should().BeTrue();
        errors.Any(x => x.Contains("https://github.com/oidcproxydotnet/OidcProxy.Net/wiki/Errors#gcn-az-", StringComparison.InvariantCulture)).Should().BeTrue();
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
        errors.Any(x => x.Contains("-AZ-", StringComparison.InvariantCulture)).Should().BeTrue();
        errors.Any(x => x.Contains("https://github.com/oidcproxydotnet/OidcProxy.Net/wiki/Errors#gcn-az-", StringComparison.InvariantCulture)).Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void WhenInvalidTenantId_ShouldThrowException(string invalid)
    {
        _config.TenantId = invalid;

        var actual = _config.Validate(out var errors);

        actual.Should().BeFalse();
        errors.Any(x => x.Contains("tenantid", StringComparison.InvariantCultureIgnoreCase)).Should().BeTrue();
        errors.Any(x => x.Contains("-AZ-", StringComparison.InvariantCulture)).Should().BeTrue();
        errors.Any(x => x.Contains("https://github.com/oidcproxydotnet/OidcProxy.Net/wiki/Errors#gcn-az-", StringComparison.InvariantCulture))
            .Should().BeTrue();
    }
}