using PuppeteerSharp;

namespace Host.TestApps.Auth0.IntegrationTests.Pom;

public class MeEndpoint : Endpoint
{
    public MeEndpoint(IPage page) : base(page)
    {
    }
}

public class EchoEndpoint : Endpoint
{
    public EchoEndpoint(IPage page) : base(page)
    {
    }
}

public class Endpoint
{
    private readonly IPage _page;

    public Endpoint(IPage page)
    {
        _page = page;
    }

    public string Text => _page.GetContentAsync().GetAwaiter().GetResult();
}