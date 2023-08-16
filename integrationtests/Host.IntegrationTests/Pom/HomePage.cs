using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class HomePage
{
    private readonly IPage _page;

    public HomePage(IPage page)
    {
        _page = page;
    }

    public IElementHandle BtnOidcLogin => GetElementHandle("#btn-oidc-login");
    public IElementHandle BtnAuth0Login => GetElementHandle("#btn-auth0-login");

    private IElementHandle GetElementHandle(string selector) 
        => _page.QuerySelectorAsync(selector).GetAwaiter().GetResult();
}