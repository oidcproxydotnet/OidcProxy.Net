using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.DependencyInjection;
using OidcProxy.Net.Locking;
using OidcProxy.Net.Locking.Distributed.Redis;
using OidcProxy.Net.Locking.InMemory;
using RedLockNet;
using RedLockNet.SERedis;
using RedLockNet.SERedis.Configuration;
using StackExchange.Redis;

namespace OidcProxy.Net.ModuleInitializers.Configuration;

public class SessionBootstrap(TimeSpan sessionIdleTimeout, string cookieName) : IBootstrap
{
    private ConnectionMultiplexer? _connectionMultiplexer;

    public SessionBootstrap WithRedis(ConnectionMultiplexer connectionMultiplexer)
    {
        _connectionMultiplexer = connectionMultiplexer;
        return this;
    }

    public void Configure(ProxyOptions options, IServiceCollection services)
    {
        services
            .AddDistributedMemoryCache()
            .AddMemoryCache()
            .AddSession(options =>
            {
                options.IdleTimeout = sessionIdleTimeout;
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
                options.Cookie.Name = cookieName;
            });

        if (_connectionMultiplexer == null)
        {
            services.AddTransient<IConcurrentContext, InMemoryConcurrentContext>();
        }
        else
        {
            services
                .AddDataProtection()
                .PersistKeysToStackExchangeRedis(_connectionMultiplexer, cookieName);

            services.AddStackExchangeRedisCache(redisCacheOptions =>
            {
                redisCacheOptions.Configuration = _connectionMultiplexer.Configuration;
                redisCacheOptions.InstanceName = cookieName;
            });

            services
                .AddTransient<IConcurrentContext, RedisConcurrentContext>()
                .AddTransient<IDistributedLockFactory>(_ => RedLockFactory.Create(new List<RedLockMultiplexer>() { _connectionMultiplexer }));
        }
    }

    public void Configure(ProxyOptions options, WebApplication app)
    {
        app.UseSession();
        
        app.Use(async (context, next) =>
        {
            await context.Session.LoadAsync();
            await next();
        });
    }
}