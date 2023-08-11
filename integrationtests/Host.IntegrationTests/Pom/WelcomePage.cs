using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class WelcomePage
{    
    private readonly IPage _page;

    public WelcomePage(IPage page)
    {
        _page = page;
    }

    public static string Uri = "/account/welcome";

    public IElementHandle BtnOidcMe => GetElement("#btn-oidc-me");
    
    public IElementHandle BtnEcho => GetElement("#btn-echo");

    private IElementHandle GetElement(string selector) => _page.QuerySelectorAsync(selector).GetAwaiter().GetResult();
}