using System.Net;
using PuppeteerSharp;

namespace Host.TestApps.IntegrationTests.Pom;

public class App
{
    private IPage? _page;
    private IBrowser? _browser;
    private IResponse? _response;

    private const string BaseAddress = "https://localhost:8444";

    public async Task NavigateToProxy(string startUrl = "/.auth/login")
    {
        using var browserFetcher = new BrowserFetcher(SupportedBrowser.Chrome);
        await browserFetcher.DownloadAsync();

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = true,
            IgnoreHTTPSErrors = true,
            Args = ["--no-sandbox"]
        });
        
        _page = await _browser.NewPageAsync();
        
        await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3419.0 Safari/537.36");

        _response = await _page.GoToAsync($"{BaseAddress}{startUrl}");
    }

    public async Task GoTo(string uri)
    {
        _response = await _page!.GoToAsync($"{BaseAddress}{uri}");
    }

    public async Task CloseBrowser()
    {
        await _page!.CloseAsync();
        await _browser!.CloseAsync();
    }

    public string CurrentUrl => _page?.Url ?? string.Empty;

    public HttpStatusCode CurrentStatus => _response.Status;
    
    public Endpoint CurrentPage => new(_page!);
}