using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using NSubstitute;
using OidcProxy.Net.Logging;

namespace OidcProxy.Net.OpenIdConnect.Tests.OpenIdConnectIdentityProviderTests;

public class RefreshTokenTests
{
    private const string TraceIdentifier = "test";
    private readonly HttpClient _httpClient;
    private readonly OpenIdConnectConfig _config;
    private readonly TestCache _cache;    
    private readonly ILogger _logger = Substitute.For<ILogger>();


    public RefreshTokenTests()
    {
        _httpClient = new WebApplicationFactory<TestProgram>()
            .CreateClient();
        
        _config = new OpenIdConnectConfig
        {
            Authority = _httpClient!.BaseAddress!.ToString()
        };
        
        _cache = new TestCache();
    }
    
    [Fact]
    public async Task ShouldApplyExpiresInValueFromTokenResponse()
    {
        TestProgram.AccessTokenResponse = @"{
                ""access_token"":""MTQ0NjJkZmQ5OTM2NDE1ZTZjNGZmZjI3"",
                ""id_token"":""MTQ0NjJkZmQ5OTM2NDE1ZTZjNGZmZjI3"",
                ""token_type"":""Bearer"",
                ""expires_in"":3600,
                ""refresh_token"":""IwOGYzYTlmM2YxOTQ5MGE3YmNmMDFkNTVk"",
                ""scope"":""create""
            }";
        
        var expected = DateTime.UtcNow.AddSeconds(3600);
        var sut = new OpenIdConnectIdentityProvider(_logger, _cache, _httpClient, _config);

        var tokenResponse = await sut.RefreshTokenAsync("test", TraceIdentifier);

        var actual = tokenResponse.ExpiryDate - expected;

        actual.TotalSeconds.Should().BeLessThan(10);
    }
}