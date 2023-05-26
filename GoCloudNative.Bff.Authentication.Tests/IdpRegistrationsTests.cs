using FluentAssertions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

namespace GoCloudNative.Bff.Authentication.Tests;

public class IdpRegistrationsTests
{
    [Fact]
    public void WhenRegisteringEndpointNameTwice_ShouldThrowNotSupportedException()
    {
        // Arrange
        var sut = new IdpRegistrations();
        sut.Register<TestIdp1, TestConfig>(new TestConfig(), "test");
        
        // Act
        Action actual = () => sut.Register<TestIdp2, TestConfig>(new TestConfig(), "test");

        // Assert
        actual.Should().Throw<NotSupportedException>();
    }
    
    [Fact]
    public void WhenRegisteringTypeTwice_ShouldThrowNotSupportedException()
    {
        // Arrange
        var sut = new IdpRegistrations();
        sut.Register<TestIdp1, TestConfig>(new TestConfig(), "test1");
        
        // Act
        Action actual = () => sut.Register<TestIdp1, TestConfig>(new TestConfig(), "test2");

        // Assert
        actual.Should().Throw<NotSupportedException>();
    }

    private class TestConfig
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

        public Task<TokenResponse> GetTokenAsync(string redirectUri, string code, string? codeVerifier)
        {
            throw new NotImplementedException();
        }

        public Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            throw new NotImplementedException();
        }

        public Task Revoke(string token)
        {
            throw new NotImplementedException();
        }

        public Task<Uri> GetEndSessionEndpoint(string? idToken, string baseAddress)
        {
            throw new NotImplementedException();
        }
    }
}