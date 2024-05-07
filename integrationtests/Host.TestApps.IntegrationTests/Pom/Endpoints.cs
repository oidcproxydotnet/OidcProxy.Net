using PuppeteerSharp;

namespace Host.TestApps.IntegrationTests.Pom;

public class Endpoint
{
    private readonly IPage _page;

    public Endpoint(IPage page)
    {
        _page = page;
    }

    public string Text => _page.GetContentAsync().GetAwaiter().GetResult();
}