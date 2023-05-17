using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests;

public class TestCache : IMemoryCache
{
    private List<TestCacheEntry> _cache = new();
    
    public void Dispose()
    {
        
    }

    public ICacheEntry CreateEntry(object key)
    {
        var entry = new TestCacheEntry(key);
        _cache.Add(entry);
        return entry;
    }

    public void Remove(object key)
    {
        var item = _cache.SingleOrDefault(x => x.Key == key);
        if (item != null)
        {
            _cache.Remove(item);
        }
    }

    public bool TryGetValue(object key, out object value)
    {
        var cachedItem = _cache.SingleOrDefault(x => x.Key == key);
        value = cachedItem?.Value;
        return value != null;
    }

    private class TestCacheEntry : ICacheEntry
    {
        public TestCacheEntry(object key)
        {
            Key = key;
        }
        
        public DateTimeOffset? AbsoluteExpiration { get; set; } = DateTimeOffset.Now.AddDays(1);
        public TimeSpan? AbsoluteExpirationRelativeToNow { get; set; } = TimeSpan.FromDays(1);
        public IList<IChangeToken> ExpirationTokens { get; } = new List<IChangeToken>();
        public object Key { get; }

        public IList<PostEvictionCallbackRegistration> PostEvictionCallbacks { get; } =
            new List<PostEvictionCallbackRegistration>();

        public CacheItemPriority Priority { get; set; } = CacheItemPriority.Normal;
        public long? Size { get; set; }
        public TimeSpan? SlidingExpiration { get; set; } = TimeSpan.FromDays(1);
        public object Value { get; set; }
        
        public void Dispose()
        {
        }
    }
}