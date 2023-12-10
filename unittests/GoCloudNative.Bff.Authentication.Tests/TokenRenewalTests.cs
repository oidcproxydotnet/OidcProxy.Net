using FluentAssertions;
using GoCloudNative.Bff.Authentication.IdentityProviders;
using GoCloudNative.Bff.Authentication.Locking;
using GoCloudNative.Bff.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace GoCloudNative.Bff.Authentication.Tests;

public class TokenRenewalTests : IAsyncLifetime
{
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private readonly ISession _session = new TestSession();

    private readonly SingleInstance _context = new (); 

    public TokenRenewalTests()
    {
        _identityProvider.RefreshTokenAsync(Arg.Any<string>()).Returns(Task.Run(async () =>
        {
            await Task.Delay(250);
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
    }

    public Task DisposeAsync()
    {
        // i.l.e.

        return Task.CompletedTask;
    }
    
    [Fact]
    public async Task When1000ConcurrentRequests_ShouldRenewTokenOnce()
    {
        var tasks = new List<Task>();
        for (var i = 0; i < 1000; i++)
        {
            tasks.Add(GetToken());
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1).RefreshTokenAsync(Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_identityProvider, _session, _context);
            await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
        }
    }
    
    [Fact]
    public async Task ShouldUpdateRefreshToken()
    {   
        // Arrange
        var sut = new TokenFactory(_identityProvider, _session, _context);
        
        // Act
        await sut.RenewAccessTokenIfExpiredAsync<IIdentityProvider>();
        
        // Assert
        _session.GetString("token_key").Should().NotBeEmpty();
        _session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
}