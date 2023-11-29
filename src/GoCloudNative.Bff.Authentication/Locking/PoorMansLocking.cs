using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Locking;

public class PoorMansLocking : ILocker
{
    public async Task AcquireLock(ISession session, int timeout, Func<Task> @delegate)
    {
        var cacheKey = $"{session.Id}+{typeof(PoorMansLocking).FullName}+isLocked";
        
        try
        {
            const int idle = 100;
            var totalTimeWaited = 0;
            for (; 
                 totalTimeWaited < timeout && session.Keys.Contains(cacheKey); 
                 totalTimeWaited += idle)
            {
                await Task.Delay(100);
            }

            if (totalTimeWaited >= timeout)
            {
                throw new TimeoutException();
            }

            if (!TryAcquireLock(session, cacheKey))
            {
                await AcquireLock(session, timeout - totalTimeWaited, @delegate);
                return;
            }

            await @delegate();
        }
        finally
        {
            await session.RemoveAsync(cacheKey);
        }
    }

    private static bool TryAcquireLock(ISession session, string cacheKey)
    {
        var guid = Guid.NewGuid().ToByteArray();
        if (session.Keys.Contains(cacheKey))
        {
            return false;
        }

        session.Set(cacheKey, guid);
        return true;
    }
}