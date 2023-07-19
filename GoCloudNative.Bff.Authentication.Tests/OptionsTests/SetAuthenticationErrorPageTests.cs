using FluentAssertions;
using GoCloudNative.Bff.Authentication.ModuleInitializers;

namespace GoCloudNative.Bff.Authentication.Tests.OptionsTests;

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
        var sut = new BffOptions();
        
        sut.SetAuthenticationErrorPage(relativePath);

        sut.ErrorPage.Should().Be(relativePath);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("https://external.domain.com")]
    [InlineData("/error?")]
    [InlineData("/error?q=x")]
    [InlineData("/error#")]
    [InlineData("/error#q=x")]
    public void InvalidRelativePath_ShouldThrowArgumentException(string path)
    {
        var sut = new BffOptions();
        
        var actual = () => sut.SetAuthenticationErrorPage(path);

        actual.Should().Throw<NotSupportedException>();
    }
}