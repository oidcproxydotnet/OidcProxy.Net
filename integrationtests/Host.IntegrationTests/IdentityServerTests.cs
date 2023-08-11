using Host.IntegrationTests.Fixtures;
using Xunit;

namespace Host.IntegrationTests;

public class IdentityServerTests : IClassFixture<HostAndIdsvrFixture>
{
    [Fact]
    public async Task ItShouldWork()
    {
        await Task.Delay(15000);

        Assert.True(true);
    }
}