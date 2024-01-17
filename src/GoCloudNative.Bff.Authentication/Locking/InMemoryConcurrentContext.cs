using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Locking;

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

            var s = new SemaphoreSlim(1);
            await s.WaitAsync();

            s.Release();
            await semaphore.WaitAsync();
            if (actionRequired())
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