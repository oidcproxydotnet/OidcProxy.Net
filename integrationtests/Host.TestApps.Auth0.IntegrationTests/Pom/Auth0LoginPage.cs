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
    
    private IElementHandle GetElement(string selector) => _page.QuerySelectorAsync(selector).GetAwaiter().GetResult();

    public async Task HitEnter()
    {
        await _page.Keyboard.PressAsync("Enter");
    }
}