using PuppeteerSharp;

namespace Host.TestApps.Auth0.IntegrationTests.Pom;

public class Auth0LoginPage
{
    private readonly IPage _page;

    public Auth0LoginPage(IPage page)
    {
        _page = page;
    }

    public IElementHandle TxtUsername => GetElement("#username");
    
    public IElementHandle TxtPassword => GetElement("#password");

    public IElementHandle BtnContinue => GetElement("section div div form div.c22fea258 button[type='submit']");

    private IElementHandle GetElement(string selector) => _page.QuerySelectorAsync(selector).GetAwaiter().GetResult();
}