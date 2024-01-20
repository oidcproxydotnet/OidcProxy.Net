using PuppeteerSharp;

namespace Host.TestApps.Auth0.IntegrationTests.Pom;

public class App
{
    private IPage? _page;
    private IBrowser _browser;

    private const string BaseAddress = "https://localhost:8443";

    public async Task NavigateToProxy()
    {
        using var browserFetcher = new BrowserFetcher(SupportedBrowser.Chrome);
        await browserFetcher.DownloadAsync();

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            IgnoreHTTPSErrors = true,
            Args = new []{ "--no-sandbox" }
        });
        
        _page = await _browser.NewPageAsync();
        await _page.GoToAsync($"{BaseAddress}/.auth/login");
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

    public Auth0LoginPage Auth0LoginPage => new(_page);
    public Auth0SignOutPage Auth0SignOutPage => new(_page);
    
    
    public MeEndpoint MeEndpoint => new(_page);
    public EchoEndpoint EchoEndpoint => new(_page);
}