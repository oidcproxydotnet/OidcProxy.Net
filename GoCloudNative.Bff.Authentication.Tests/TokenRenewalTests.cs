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
        _identityProvider.RefreshTokenAsync(Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            await Task.Delay(25);
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
            var bogusToken = Guid.NewGuid().ToString();
            var sut = new TokenRenewer(_identityProvider, _session);
            await sut.Renew(bogusToken);
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRefreshToken()
    {   
        // Arrange
        var bogusToken = Guid.NewGuid().ToString();
        var sut = new TokenRenewer(_identityProvider, _session);
        
        // Act
        await sut.Renew(bogusToken);
        
        // Assert
        _session.GetString("token_key").Should().NotBeEmpty();
        _session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task ShouldThrowTimeOut()
    {
        // Arrange
        var bogusToken = Guid.NewGuid().ToString();
        var sut = new TokenRenewer(_identityProvider, _session);
        
        var cacheKey = $"refreshing_token_{_session.Id}";
        _session.SetString(cacheKey, "true");
        
        // Act
        Func<Task> actual = async () => await sut.Renew(bogusToken, 3);
        
        // Assert
        _identityProvider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());

        await actual.Should().ThrowAsync<TimeoutException>();
    }


    [Fact]
    public async Task ShouldNotThrowTimeOut()
    {
        // Arrange
        var bogusToken = Guid.NewGuid().ToString();
        var sut = new TokenRenewer(_identityProvider, _session);

        var cacheKey = $"refreshing_token_{_session.Id}";
        _session.SetString(cacheKey, "true");

        // Act
        Task.Run(async () =>
        {
            await Task.Delay(1000);

            _session.Remove(cacheKey);
        });

        Func<Task> actual = async () => await sut.Renew(bogusToken);

        // Assert
        _identityProvider.DidNotReceive().RefreshTokenAsync(Arg.Any<string>());
        
        await actual.Should().NotThrowAsync<TimeoutException>();
    }
}