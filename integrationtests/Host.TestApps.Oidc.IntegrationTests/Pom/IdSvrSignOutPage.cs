using PuppeteerSharp;

namespace Host.TestApps.Oidc.IntegrationTests.Pom;

public class IdSvrSignOutPage
{
    private readonly IPage _page;

    public IdSvrSignOutPage(IPage page)
    {
        _page = page;
    }

    public IElementHandle BtnYes => _page.QuerySelectorAsync("#yes").GetAwaiter().GetResult();
}