using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class Auth0SignOutPage
{
    private readonly IPage _page;

    public Auth0SignOutPage(IPage page)
    {
        _page = page;
    }

    public IElementHandle BtnAccept => GetElement("button[value='accept']");

    private IElementHandle GetElement(string selector) => _page.QuerySelectorAsync(selector).GetAwaiter().GetResult();
}