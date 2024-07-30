using FluentAssertions;
using OidcProxy.Net.Cryptography;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Tests.UnitTests.OptionsTests;

public class RegisterIdentityProviderTests
{
    [Fact]
    public void WhenRegisteringTypeTwice_ShouldThrowNotSupportedException()
    {
        // Arrange
        var sut = new ProxyOptions();
        sut.RegisterIdentityProvider<TestIdp1, TestConfig>(new TestConfig());
        
        // Act
        Action actual = () => sut.RegisterIdentityProvider<TestIdp1, TestConfig2>(new TestConfig2());

        // Assert
        actual.Should().Throw<NotSupportedException>();
    }

    [Fact]
    public void WhenRegisteringOptionsTypeTwice_ShouldThrowNotSupportedException()
    {
        // Arrange
        var sut = new ProxyOptions();
        sut.RegisterIdentityProvider<TestIdp1, TestConfig>(new TestConfig());
        
        // Act
        Action actual = () => sut.RegisterIdentityProvider<TestIdp2, TestConfig>(new TestConfig());

        // Assert
        actual.Should().Throw<NotSupportedException>();
    }
    
    private class TestConfig
    {
    }

    private class TestConfig2
    {
    }
    
    private class TestIdp2 : TestIdp1
    {
    }

    private class TestIdp1 : IIdentityProvider
    {
        public Task<AuthorizeRequest> GetAuthorizeUrlAsync(string redirectUri)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier, string traceIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<KeySet>> GetJwksAsync(bool invalidateCache)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> RefreshTokenAsync(string refreshToken, string traceIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task RevokeAsync(string token, string traceIdentifier)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> GetEndSessionEndpointAsync(string? idToken, string baseAddress)
        {
            throw new NotImplementedException();
        }
    }
}