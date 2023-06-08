using FluentAssertions;
using GoCloudNative.Bff.Authentication.ModuleInitializers;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Tests;

public class RedirectUriFactoryDetermineHostNameTests
{   
    [Theory]
    [InlineData("http://localhost/", "https://localhost")]
    [InlineData("http://localhost:80", "https://localhost")]
    [InlineData("https://localhost:443", "https://localhost")]
    [InlineData("http://localhost/account/login/redirect", "https://localhost")]
    [InlineData("http://localhost:8443/account/login/redirect", "https://localhost:8443")]
    [InlineData("HTTP://localhost:8443/account/login/redirect", "https://localhost:8443")]
    [InlineData("https://localhost:8443/account/login/redirect", "https://localhost:8443")]
    public void WhenHttp_ShouldRewriteToHttps(string url, string expected)
    {   
        var options = new BffOptions();
        options.AlwaysRedirectToHttps = true;

        var _httpContext = CreateHttpContext(url);
        
        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName(_httpContext);

        actual.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("http://localhost/", "http://localhost")]
    [InlineData("http://localhost:80", "http://localhost")]
    [InlineData("https://localhost:443", "https://localhost")]
    [InlineData("http://localhost/account/login/redirect", "http://localhost")]
    [InlineData("http://localhost:8443/account/login/redirect", "http://localhost:8443")]
    [InlineData("HTTP://localhost:8443/account/login/redirect", "http://localhost:8443")]
    [InlineData("https://localhost:8443/account/login/redirect", "https://localhost:8443")]
    public void WhenRewriteModeSetToFalseHttp_ShouldRewriteToHttps(string url, string expected)
    {   
        var options = new BffOptions();
        options.AlwaysRedirectToHttps = false;

        var _httpContext = CreateHttpContext(url);
        
        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName(_httpContext);

        actual.Should().Be(expected);
    }

    [Theory]
    [InlineData("http://localhost/", "https://localhost")]
    [InlineData("http://localhost:80", "https://localhost")]
    [InlineData("http://localhost/account/login/redirect", "https://localhost")]
    [InlineData("http://localhost:8443/account/login/redirect", "https://localhost:8443")]
    [InlineData("HTTP://localhost:8443/account/login/redirect", "https://localhost:8443")]
    [InlineData("https://localhost:8443/account/login/redirect", "https://localhost:8443")]
    public void WhenCustomHostNameHttp_ShouldRewriteToHttps(string url, string expected)
    {   
        var options = new BffOptions();
        options.AlwaysRedirectToHttps = true;
        options.SetCustomHostName(new Uri(url));

        var _httpContext = CreateHttpContext("http://localhost");
        
        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName(_httpContext);

        actual.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("https://localhost:443", "https://localhost")]
    [InlineData("http://localhost:8443/account/login/redirect", "http://localhost:8443")]
    [InlineData("HTTP://localhost:8443/account/login/redirect", "http://localhost:8443")]
    [InlineData("https://localhost:8443/account/login/redirect", "https://localhost:8443")]
    public void WhenCustomHostNameAndRewriteModeSetToFalse_ShouldNotRewriteToHttps(string url, string expected)
    {   
        var options = new BffOptions();
        options.AlwaysRedirectToHttps = false;
        options.SetCustomHostName(new Uri(url));

        var _httpContext = CreateHttpContext("http://localhost");
        
        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName(_httpContext);

        actual.Should().Be(expected);
    }


    private static HttpContext CreateHttpContext(string url)
    {
        var uri = new Uri(url);
        
        return new DefaultHttpContext
        {
            Request =
            {
                Method = "GET",
                Scheme = uri.Scheme,
                Host = new HostString(uri.Authority),
                Path = uri.AbsolutePath,
                QueryString = new QueryString(uri.Query)
            }
        };
    }
}