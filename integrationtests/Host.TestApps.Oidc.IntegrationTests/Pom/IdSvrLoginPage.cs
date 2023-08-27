using PuppeteerSharp;

namespace Host.TestApps.Oidc.IntegrationTests.Pom;

public class IdSvrLoginPage
{
    private readonly IPage _page;

    public IdSvrLoginPage(IPage page)
    {
        _page = page;
    }

    public IElementHandle BtnYodaLogin => _page.QuerySelectorAsync("#yoda").GetAwaiter().GetResult();
}