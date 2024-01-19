using Microsoft.AspNetCore.Http;
using RedLockNet;

namespace OidcProxy.Net.Locking.Distributed.Redis;

public class RedisConcurrentContext : IConcurrentContext
{
    private readonly IDistributedLockFactory _redisLockFactory;

    public RedisConcurrentContext(IDistributedLockFactory redisLockFactory)
    {
        _redisLockFactory = redisLockFactory;
    }
    
    public async Task ExecuteOncePerSession(ISession session, string identifier, Func<bool> actionRequired, Func<Task> @delegate)
    {
        var expiryTime = TimeSpan.FromSeconds(15);
        var waitTime = TimeSpan.FromMilliseconds(100);
        var retryTime = TimeSpan.FromSeconds(5);
        
        if (!actionRequired())
        { 
            return;
        }
        
        var cacheKey = $"{typeof(RedisConcurrentContext).FullName}+{session.Id}+{identifier}";

        await using var resourceLock = await _redisLockFactory.CreateLockAsync(cacheKey, expiryTime, waitTime, retryTime);
        
        var isActionRequired = actionRequired();
        if (!resourceLock.IsAcquired && isActionRequired)
        {
            throw new ApplicationException($"Unable to renew the expired access_token. Unable to acquire a lock. " +
                                           $"Try again. Error: {resourceLock.InstanceSummary.ToString()}");
        }
        
        if (!isActionRequired)
        { 
            return;
        }

        await @delegate();
    }
}