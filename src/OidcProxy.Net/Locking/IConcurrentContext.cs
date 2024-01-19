using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Locking;

public interface IConcurrentContext
{
    /// <summary>
    /// Executes a task once per session
    /// </summary>
    /// <param name="session">A reference to ISession. Primarily used to create a lock based on the session id.</param>
    /// <param name="identifier">A unique string that identifies the process you want to run once.</param>
    /// <param name="actionRequired">A delegate that is used to determine if it is (still) necessary to execute the workload.</param>
    /// <param name="delegate">The workload that needs to be executed.</param>
    /// <returns>An awaitable task.</returns>
    Task ExecuteOncePerSession(ISession session, string identifier, Func<bool> actionRequired, Func<Task> @delegate);
}