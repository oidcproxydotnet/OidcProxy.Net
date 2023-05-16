using FluentAssertions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TokenRenewalTests
{
    private IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private IDistributedCache _cache;

    private ISession _session = new TestSession();

    public TokenRenewalTests()
    {
        var cacheOptions = Options.Create(new MemoryDistributedCacheOptions());
        _cache = new MemoryDistributedCache(cacheOptions);
        
        _identityProvider.RefreshTokenAsync(Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            await Task.Delay(25);
            return new TokenResponse(Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString());
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
            var sut = new TokenRenewer(_identityProvider, _cache, _session);
            await sut.Renew(bogusToken);
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRefreshToken()
    {
        // Arrange
        var bogusToken = Guid.NewGuid().ToString();
        var sut = new TokenRenewer(_identityProvider, _cache, _session);
        
        // Act
        await sut.Renew(bogusToken);
        
        // Assert
        _session.GetString("token_key").Should().NotBeEmpty();
        _session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
}