using FluentAssertions;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Tests.OptionsTests;

public class SetLandingPageTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/error")]
    [InlineData("/Err0r")]
    [InlineData("/error/")]
    [InlineData("/error.html")]
    [InlineData("/oh/my/something%20really-bad has_happened")]
    [InlineData("/error?")]
    [InlineData("/error?q=x")]
    [InlineData("/error#")]
    [InlineData("/error#q=x")]
    public void ValidRelativePath_ShouldSetLandingPage(string relativePath)
    {
        var sut = new ProxyOptions();
        
        sut.SetLandingPage(relativePath);

        sut.LandingPage.ToString().Should().Be(relativePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://external.domain.com")]
    public void InvalidRelativePath_ShouldThrowNotSupportedException(string path)
    {
        var sut = new ProxyOptions();
        
        var actual = () => sut.SetLandingPage(path);

        actual.Should().Throw<NotSupportedException>();
    }
}