using GoCloudNative.Bff.Authentication.Locking;

namespace GoCloudNative.Bff.Authentication.Tests.Locking;

public class PoorMansLockingTest
{
    [Fact]
    public async Task ItShouldExecuteCriticalSectionOnce()
    {
        var session = new TestSession();
        var isInUse = false;

        var output = new List<string>();
        
        var tasks = new List<Func<Task>>();
        for (var i = 0; i < 10; i++)
        {
            tasks.Add(async () =>
            {
                var locker = new PoorMansLocking();

                await locker.AcquireLock(session, 5000, async () =>
                {
                    output.Add(DateTime.Now.Millisecond.ToString());
                    if (isInUse)
                    {
                        Assert.Fail("If locking would have worked properly, this would never happen.");
                    }

                    output.Add("true");
                    isInUse = true;
                    await Task.Delay(50);
                    output.Add("false");
                    isInUse = false;
                });
            });
        }

        await Task.WhenAll(tasks.Select(payload => payload()));
    }
}