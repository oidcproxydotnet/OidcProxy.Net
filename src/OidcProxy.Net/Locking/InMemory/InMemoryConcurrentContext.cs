using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Locking.InMemory;

internal class InMemoryConcurrentContext : IConcurrentContext
{   
    public async Task ExecuteOncePerSession(ISession session, string identifier, Func<bool> actionRequired, Func<Task> @delegate)
    {
        var cacheKey = $"{typeof(InMemoryConcurrentContext).FullName}+{session.Id}+{identifier}";

        var semaphore = Semaphores.GetInstance(cacheKey);

        try
        {
            if (!actionRequired())
            { 
                return;
            }
            
            var acquired = await semaphore.WaitAsync(TimeSpan.FromSeconds(15));
            if (acquired && actionRequired())
            {
                await @delegate();
            }
        }
        finally
        {
            semaphore.Release();
            Semaphores.RemoveInstance(cacheKey);
        }
    }
}