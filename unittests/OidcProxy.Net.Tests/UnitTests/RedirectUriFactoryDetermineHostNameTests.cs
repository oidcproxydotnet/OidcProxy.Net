using FluentAssertions;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Tests.UnitTests;

public class RedirectUriFactoryDetermineHostNameTests
{   
    [Theory]
    [InlineData("http://localhost/", "http://localhost")]
    [InlineData("http://localhost:80", "http://localhost")]
    [InlineData("https://localhost:443", "https://localhost")]
    [InlineData("http://localhost/.auth/login/redirect", "http://localhost")]
    [InlineData("http://localhost:8443/.auth/login/redirect", "http://localhost:8443")]
    [InlineData("HTTP://localhost:8443/.auth/login/redirect", "http://localhost:8443")]
    [InlineData("https://localhost:8443/.auth/login/redirect", "https://localhost:8443")]
    public void WhenRewriteModeSetToFalseHttp_ShouldRewriteToHttps(string url, string expected)
    {   
        var options = new ProxyOptions
        {
            HostName = new Uri(url)
        };

        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName();

        actual.Should().Be(expected);
    }
    
    [Theory]
    [InlineData("https://localhost:443", "https://localhost")]
    [InlineData("http://localhost:8443/.auth/login/redirect", "http://localhost:8443")]
    [InlineData("HTTP://localhost:8443/.auth/login/redirect", "http://localhost:8443")]
    [InlineData("https://localhost:8443/.auth/login/redirect", "https://localhost:8443")]
    public void WhenCustomHostNameAndRewriteModeSetToFalse_ShouldNotRewriteToHttps(string url, string expected)
    {   
        var options = new ProxyOptions
        {
            HostName = new Uri(url)
        };

        var sut = new RedirectUriFactory(options);
        
        var actual = sut.DetermineHostName();

        actual.Should().Be(expected);
    }
}