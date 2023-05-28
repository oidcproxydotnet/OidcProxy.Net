using FluentAssertions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TokenRenewalTests
{
    private IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private ISession _session = new TestSession();

    public TokenRenewalTests()
    {
        _session.Save<IIdentityProvider>(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.Now.AddSeconds(-1)));
        
        _identityProvider.RefreshTokenAsync(Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            await Task.Delay(250);
            return new TokenResponse(Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddSeconds(75));
        }));
    }
    
    [Fact]
    public async Task When1000ConcurrentRequests_ShouldRenewTokenOnce()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(GetToken());
        }

        Task.WhenAll(tasks);

        _identityProvider.Received(1).RefreshTokenAsync(Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session);
            await sut.RenewAccessTokenIfExpired<IIdentityProvider>();
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRefreshToken()
    {   
        // Arrange
        var sut = new TokenFactory(_identityProvider, _session);
        
        // Act
        await sut.RenewAccessTokenIfExpired<IIdentityProvider>();
        
        // Assert
        _session.GetString("token_key").Should().NotBeEmpty();
        _session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task ShouldThrowTimeOut()
    {
        // Arrange
        var bogusToken = Guid.NewGuid().ToString();
        var sut = new TokenFactory(_identityProvider, _session);
        
        var cacheKey = $"refreshing_token_{_session.Id}";
        _session.SetString(cacheKey, "true");
        
        // Act
        Func<Task> actual = async () => await sut.RenewAccessTokenIfExpired<IIdentityProvider>(2);
        
        // Assert
        _identityProvider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());

        await actual.Should().ThrowAsync<TimeoutException>();
    }

    [Fact]
    public async Task ShouldNotThrowTimeOut()
    {
        // Arrange
        var sut = new TokenFactory(_identityProvider, _session);

        var cacheKey = $"refreshing_token_{_session.Id}";
        _session.SetString(cacheKey, "true");

        // Act
        Task.Run(async () =>
        {
            await Task.Delay(1000);

            _session.Remove(cacheKey);
        });

        Func<Task> actual = async () => await sut.RenewAccessTokenIfExpired<IIdentityProvider>();

        // Assert
        _identityProvider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());
        
        await actual.Should().NotThrowAsync<TimeoutException>();
    }
}