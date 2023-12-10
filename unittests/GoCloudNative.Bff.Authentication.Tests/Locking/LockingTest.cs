using GoCloudNative.Bff.Authentication.Locking;

namespace GoCloudNative.Bff.Authentication.Tests.Locking;

public class LockingTest
{
    [Fact]
    public async Task ItShouldExecuteCriticalSectionOnce()
    {
        var session = new TestSession();
        var isInUse = false;
        var somethingChanged = false;

        var tasks = new List<Func<Task>>();
        for (var i = 0; i < 2500; i++)
        {
            tasks.Add(async () =>
            {
                var concurrentContext = new SingleInstance();
                await concurrentContext.ExecuteOncePerSession(session, "foo",
                    () => !somethingChanged,
                    async () =>
                    {
                        if (isInUse)
                        {
                            Assert.Fail("If locking would have worked properly, this would never happen.");
                        }

                        isInUse = true;
                        await Task.Delay(50);
                        isInUse = false;

                        somethingChanged = true;
                    });
                });
        }

        await Task.WhenAll(tasks.Select(p => p()));
    }
}