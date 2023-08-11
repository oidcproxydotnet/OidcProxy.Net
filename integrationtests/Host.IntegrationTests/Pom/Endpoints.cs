using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class MeEndpoint : Endpoint
{
    public static string GetUri(string protocol) => $"/{protocol}/me";

    public MeEndpoint(IPage page) : base(page)
    {
    }
}

public class EchoEndpoint : Endpoint
{
    public static string Uri = "/echo";
    
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