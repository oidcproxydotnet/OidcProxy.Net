using FluentAssertions;
using PuppeteerSharp;
using TechTalk.SpecFlow;

namespace Host.TestApps.IntegrationTests.Specs.Glue;

[Binding]
public class BrowserInteractionSteps(ScenarioContext scenarioContext)
{
    private IBrowser _browser;
    private IPage _page;
    private IResponse _response;

    [Given("the user interacts with the site that implements the OidcProxy with a browser")]
    public async Task StartBrowser()
    {   
        using var browserFetcher = new BrowserFetcher(SupportedBrowser.Chrome);
        await browserFetcher.DownloadAsync();

        _browser = await Puppeteer.LaunchAsync(new LaunchOptions
        {
            Headless = false,
            IgnoreHTTPSErrors = true,
            Args = ["--no-sandbox"]
        });
        
        _page = await _browser.NewPageAsync();
        
        await _page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3419.0 Safari/537.36");
    }

    [When("the user navigates to (.*)")]
    public async Task NavigateTo(string path)
    {
        var root = scenarioContext["proxyurl"];
        _response = await _page.GoToAsync($"{root}{path}");
    }

    [Then("the path in the browser equals (.*)")]
    public void AssertUrlEquals(string url)
    {
        var uri = new Uri(_page.Url);
        var root = uri.GetLeftPart(UriPartial.Authority);
        
        _page.Url.Replace(root, string.Empty).Should().Be(url);
    }

    [Then("access_tokens are forwarded to downstream APIs")]
    public async Task AssertTokensAreForwarded()
    {
        var uri = new Uri(_page.Url);
        var root = uri.GetLeftPart(UriPartial.Authority);
        _response = await _page.GoToAsync($"{root}/api/echo");

        var content = await _page.GetContentAsync();
        content.Should().Contain("Bearer ey");
    }

    [AfterScenario]
    public async Task TearDown()
    {
        await Task.Delay(5000);
        await _page!.CloseAsync();
        await _browser!.CloseAsync();
    }
}