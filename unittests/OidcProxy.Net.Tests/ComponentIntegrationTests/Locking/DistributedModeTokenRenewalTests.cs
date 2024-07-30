using Microsoft.AspNetCore.Http;
using OidcProxy.Net.IdentityProviders;
using OidcProxy.Net.Locking.Distributed.Redis;
using OidcProxy.Net.OpenIdConnect;
using NSubstitute;
using OidcProxy.Net.Jwt.SignatureValidation;
using OidcProxy.Net.Logging;
using OidcProxy.Net.Tests.UnitTests;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;
using Testcontainers.Redis;

namespace OidcProxy.Net.Tests.ComponentIntegrationTests.Locking;

public class DistributedModeTokenRenewalTests : IAsyncLifetime
{
    private RedisContainer? _redisContainer;
    
    private RedLockFactory? _redLockFactory;

    private readonly string TraceIdentifier = "123";
    
    private readonly ILogger _logger = Substitute.For<ILogger>();
    
    private readonly AuthSession _authSession;
    
    private readonly AuthSession _session2;
    
    private readonly IIdentityProvider _identityProvider = Substitute.For<IIdentityProvider>();

    private IJwtSignatureValidator _jwtSignatureValidator;

#region Initialise
    public DistributedModeTokenRenewalTests()
    {
        _authSession = CreateAuthSession(new TestSession());
        _session2 = CreateAuthSession(new TestSession());
        
        _jwtSignatureValidator = new DummyJwtSignatureValidator();
        
        // Mock the token refresh-call
        _identityProvider.RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>()).Returns(Task.Run(async () =>
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
        await _authSession.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
        
        await _session2.SaveAsync(new TokenResponse(Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            Guid.NewGuid().ToString(),
            DateTime.UtcNow.AddSeconds(-1)));
        
        _redisContainer = new RedisBuilder()
            .WithImage("redis:7.0")
            .Build();
        
        await _redisContainer.StartAsync();
        
        var connectionString = _redisContainer?.GetConnectionString()!;
        var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
        _redLockFactory = RedLockFactory.Create(new List<RedLockMultiplexer> { connection });
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

        await _identityProvider.Received(1).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());
    
        async Task GetToken()
        {   
            var sut = new TokenFactory(_authSession, _jwtSignatureValidator, _identityProvider, _logger, new RedisConcurrentContext(_redLockFactory!));
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
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

        await _identityProvider.Received(2).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());

        async Task GetToken()
        {   
            var sut = new TokenFactory(_authSession, _jwtSignatureValidator, _identityProvider, _logger, new RedisConcurrentContext(_redLockFactory!));
            await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
        }
        
        async Task GetToken2()
        {   
            var sut = new TokenFactory(_session2, _jwtSignatureValidator, _identityProvider, _logger, new RedisConcurrentContext(_redLockFactory!));
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
                
                var sut = new TokenFactory(session, _jwtSignatureValidator, _identityProvider, _logger, new RedisConcurrentContext(_redLockFactory!));
                await sut.RenewAccessTokenIfExpiredAsync(TraceIdentifier);
            }));
        }

        await Task.WhenAll(tasks);

        await _identityProvider.Received(1000).RefreshTokenAsync(Arg.Any<string>(), Arg.Any<string>());
    }

    public async Task DisposeAsync()
    {
        await _redisContainer?.StopAsync()!;
    }
}