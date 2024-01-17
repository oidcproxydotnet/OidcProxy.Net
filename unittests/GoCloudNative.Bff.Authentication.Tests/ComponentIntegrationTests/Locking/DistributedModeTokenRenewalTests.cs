using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking.Distributed.Redis;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using GoCloudNative.Bff.Authentication.Tests.UnitTests;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace GoCloudNative.Bff.Authentication.Tests.ComponentIntegrationTests.Locking;

public class DistributedModeTokenRenewalTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    
    private RedLockFactory? _redLockFactory;
    
    private readonly ISession _session = new TestSession();
    
    private readonly ISession _session2 = new TestSession();
    
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

#region Initialise
    public DistributedModeTokenRenewalTests()
    {
        // Mock the token refresh-call
        _identityProvider.RefreshTokenAsync(Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            var random = new Random(DateTime.Now.Microsecond);
            await Task.Delay(2000 + random.Next(150));
            return new TokenResponse(Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddSeconds(75));
        }));
    }
    
    public async Task InitializeAsync()
    {
        await _session.SaveAsync<IIdentityProvider>(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.Now.AddSeconds(-1)));
        
        await _session2.SaveAsync<IIdentityProvider>(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.Now.AddSeconds(-1)));
        
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.0")
            .Build();
        
        await _redisContainer.StartAsync();
        
        var connectionString = _redisContainer?.GetConnectionString()!;
        var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        _redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> { connection });
    }
#endregion

    [Fact]
    public async Task When10000ConcurrentRequestsOnSingleSession_ShouldRenewTokenOnce()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 10000; i++)
        {
            tasks.Add(GetToken());
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1).RefreshTokenAsync(Arg.Any<string>());
    
        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session, new RedisConcurrentContext(_redLockFactory!));
            await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
        }
    }
    [Fact]
    public async Task When10000ConcurrentRequestsOnTwoSessions_ShouldRenewTokenTwice()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 10000; i++)
        {
            tasks.Add(GetToken());
            tasks.Add(GetToken2());
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(2).RefreshTokenAsync(Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session, new RedisConcurrentContext(_redLockFactory!));
            await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
        }
        
        async Task GetToken2()
        {   
            var sut = new TokenFactory(_identityProvider, _session2, new RedisConcurrentContext(_redLockFactory!));
            await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
        }
    }
    
    [Fact]
    public async Task WhenConcurrentRequestsOn1000Sessions_ShouldRenewToken1000Times()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(Task.Run(async () =>
            {
                var session = new TestSession();
                await session.SaveAsync<IIdentityProvider>(new TokenResponse(Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    DateTime.Now.AddSeconds(-1)));
                
                var sut = new TokenFactory(_identityProvider, session, new RedisConcurrentContext(_redLockFactory!));
                await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
            }));
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1000).RefreshTokenAsync(Arg.Any<string>());
    }

    public async Task DisposeAsync()
    {
        await _redisContainer?.StopAsync()!;
    }
}