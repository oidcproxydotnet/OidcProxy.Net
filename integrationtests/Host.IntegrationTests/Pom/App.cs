using PuppeteerSharp;

namespace Host.IntegrationTests.Pom;

public class App
{
    private IPage? _page;
    private IBrowser _browser;

    private const string BaseAddress = "https://localhost:8443";

    public async Task NavigateToBff()
    {
        using var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync(BrowserFetcher.DefaultChromiumRevision);
        
        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            IgnoreHTTPSErrors = true
        });
        
        _page = await _browser.NewPageAsync();
        await _page.GoToAsync(BaseAddress);
    }

    public async Task GoTo(string uri)
    {
        await _page.GoToAsync($"{BaseAddress}{uri}");
    }

    public async Task WaitForNavigationAsync()
    {
        await _page.WaitForNavigationAsync();
        await Task.Delay(500);
    }

    public async Task CloseBrowser()
    {
        await _page.CloseAsync();
        await _browser.CloseAsync();
    }

    public HomePage HomePage => new (_page);

    public WelcomePage WelcomePage => new(_page);
    
    
    public IdSvrLoginPage IdSvrLoginPage => new (_page);
    public IdSvrSignOutPage IdSvrSignOutPage => new (_page);
    
    
    public MeEndpoint MeEndpoint => new(_page);
    public EchoEndpoint EchoEndpoint => new(_page);
}