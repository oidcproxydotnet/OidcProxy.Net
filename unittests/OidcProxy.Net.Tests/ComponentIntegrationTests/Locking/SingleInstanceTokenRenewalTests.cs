using FluentAssertions;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Locking.InMemory;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using OidcProxy.Net.Tests.UnitTests;

namespace OidcProxy.Net.Tests.ComponentIntegrationTests.Locking;

public class SingleInstanceTokenRenewalTests : IAsyncLifetime
{
    private readonly string TraceIdentifier = "test";
    
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private readonly ISession _session = new TestSession();

    private readonly ISession _session2 = new TestSession();
    
    public SingleInstanceTokenRenewalTests()
    {
        // Mock the token refresh-call
        _identityProvider.RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            var random = new Random(DateTime.Now.Microsecond);
            await Task.Delay(100 + random.Next(150));
            return new TokenResponse(Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                Guid.NewGuid().ToString(),
                DateTime.UtcNow.AddSeconds(75));
        }));
    }
    
    public async Task InitializeAsync()
    {        
        // Store an access token dummy in the session
        await _session.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
        
        await _session2.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
    }

    public Task DisposeAsync()
    {
        // i.l.e.

        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task When1000ConcurrentRequestsOnSingleSession_ShouldRenewTokenOnce()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(GetToken());
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session, new InMemoryConcurrentContext());
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        }
    }
    
    [Fact]
    public async Task When1000ConcurrentRequestsOnTwoSessions_ShouldRenewTokenTwice()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(GetToken());
            tasks.Add(GetToken2());
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(2).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session, new InMemoryConcurrentContext());
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        }
        
        async Task GetToken2()
        {   
            var sut = new TokenFactory(_identityProvider, _session2, new InMemoryConcurrentContext());
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
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
                await session.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    DateTime.UtcNow.AddSeconds(-1)));
                
                var sut = new TokenFactory(_identityProvider, session, new InMemoryConcurrentContext());
                await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
            }));
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1000).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());
    }
    
    [Fact]
    public async Task ShouldUpdateRefreshToken()
    {   
        // Arrange
        var sut = new TokenFactory(_identityProvider, _session, new InMemoryConcurrentContext());
        
        // Act
        await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        
        // Assert
        _session.GetString("token_key").Should().NotBeEmpty();
        _session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
}