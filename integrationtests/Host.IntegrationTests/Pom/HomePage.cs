using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page)
    {
        _page = page;
    }

    public IElementHandle BtnOidcLogin => _page.QuerySelectorAsync("#btn-oidc-login").GetAwaiter().GetResult();
}