using FluentAssertions;
using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Locking.InMemory;
using OidcProxy.Net.OpenIdConnect;
using NSubstitute;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Logging;
using OidcProxy.Net.Tests.UnitTests;

namespace OidcProxy.Net.Tests.ComponentIntegrationTests.Locking;

public class SingleInstanceTokenRenewalTests : IAsyncLifetime
{
    private readonly string TraceIdentifier = "test";
    
    private readonly ILogger _logger = Substitute.For<ILogger>();
    
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private readonly AuthSession _authSession;
    
    private readonly AuthSession _session2;
    
    private IJwtSignatureValidator _jwtSignatureValidator;
    
    public SingleInstanceTokenRenewalTests()
    {
        _authSession = CreateAuthSession(new TestSession());
        _session2 = CreateAuthSession(new TestSession());
        
        _jwtSignatureValidator = new DummyJwtSignatureValidator();

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
        await _authSession.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
        
        await _session2.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
    }

    private static AuthSession CreateAuthSession(ISession session)
    {
        var httpContext = new DefaultHttpContext
        {
            Session = session
        };

        var contextAccessor = Substitute.For<IHttpContextAccessor>();
        contextAccessor.HttpContext.Returns(httpContext);

        return new AuthSession(contextAccessor,
            Substitute.For<IRedirectUriFactory>(),
            Substitute.For<IIdentityProvider>(),
            new EndpointName(".auth"));
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
            var sut = new TokenFactory(_authSession, _jwtSignatureValidator, _identityProvider, new InMemoryConcurrentContext());
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
            var sut = new TokenFactory(_authSession, _jwtSignatureValidator, _identityProvider, new InMemoryConcurrentContext());
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        }
        
        async Task GetToken2()
        {   
            var sut = new TokenFactory(_session2, _jwtSignatureValidator, _identityProvider, new InMemoryConcurrentContext());
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
                var session = CreateAuthSession(new TestSession());
                await session.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    Guid.NewGuid().ToString(),
                    DateTime.UtcNow.AddSeconds(-1)));
                
                var sut = new TokenFactory(session, _jwtSignatureValidator, _identityProvider, new InMemoryConcurrentContext());
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
        var session = new TestSession();

        var wrap = CreateAuthSession(new TestSession());
        await wrap.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
        
        var sut = new TokenFactory(wrap,_jwtSignatureValidator, _identityProvider, new InMemoryConcurrentContext());
        
        // Act
        await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        
        // Assert
        session.GetString("token_key").Should().NotBeEmpty();
        session.GetString("refresh_token_key").Should().NotBeEmpty();
    }
}