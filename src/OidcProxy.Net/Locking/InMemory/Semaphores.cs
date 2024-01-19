using System.Collections.Concurrent;

namespace OidcProxy.Net.Locking.InMemory;

internal static class Semaphores
{
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> Collection = new();

    public static SemaphoreSlim GetInstance(string key)
    {
        if (Collection.TryGetValue(key, out var value))
        {
            return value;
        }
        
        value = new SemaphoreSlim(1);
        if (Collection.TryAdd(key, value))
        {
            return value;
        }
        
        if (Collection.TryGetValue(key, out value))
        {
            return value;
        }
            
        throw new ApplicationException("Unable to create a lock.");

    }

    public static void RemoveInstance(string key)
    {
        Collection.Remove(key, out _);
    }
}