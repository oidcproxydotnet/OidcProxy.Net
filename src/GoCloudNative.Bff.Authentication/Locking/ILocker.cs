using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Locking;

public interface ILocker
{
    Task AcquireLock(ISession session, int timeout, Func<Task> @delegate);
}