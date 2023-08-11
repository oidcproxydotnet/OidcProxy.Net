using FluentAssertions;
using GoCloudNative.Bff.Authentication.Endpoints;
using Microsoft.AspNetCore.Http;

namespace GoCloudNative.Bff.Authentication.Tests.Endpoints;

public class PathStringRemoveQueryStringTests
{
    [Theory]
    [InlineData("/foo/bar?")]
    [InlineData("/foo/bar?q=xyz&x=y")]
    public void ShouldRemoveQueryString(string value)
    {
        var pathString = new PathString(value);

        var actual = pathString.RemoveQueryString();

        actual.ToString().Should().Be("/foo/bar");
    }
}