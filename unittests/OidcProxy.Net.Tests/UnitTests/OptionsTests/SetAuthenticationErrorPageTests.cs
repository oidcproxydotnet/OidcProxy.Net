using FluentAssertions;
using OidcProxy.Net.ModuleInitializers;

namespace OidcProxy.Net.Tests.OptionsTests;

public class SetAuthenticationErrorPageTests
{
    [Theory]
    [InlineData("/")]
    [InlineData("/error")]
    [InlineData("/Err0r")]
    [InlineData("/error/")]
    [InlineData("/error.html")]
    [InlineData("/oh/my/something%20really-bad has_happened")]
    public void ValidRelativePath_ShouldSetErrorPage(string relativePath)
    {
        var sut = new ProxyOptions();
        
        sut.SetAuthenticationErrorPage(relativePath);

        sut.ErrorPage.ToString().Should().Be(relativePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://external.domain.com")]
    [InlineData("/error?")]
    [InlineData("/error?q=x")]
    [InlineData("/error#")]
    [InlineData("/error#q=x")]
    public void InvalidRelativePath_ShouldThrowNotSupportedException(string path)
    {
        var sut = new ProxyOptions();
        
        var actual = () => sut.SetAuthenticationErrorPage(path);

        actual.Should().Throw<NotSupportedException>();
    }
}