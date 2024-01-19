using FluentAssertions;
using OidcProxy.Net.Endpoints;
using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Tests.UnitTests.Endpoints;

public class PathStringTrimEndTests
{
    [Theory]
    [InlineData("/foo/login", "/foo")]
    [InlineData("/foo/login/", "/foo")]
    [InlineData("/FOO/login", "/foo")]
    [InlineData("/FOO/login/", "/foo")]
    [InlineData("/foo/bar/login", "/foo/bar")]
    public void ShouldTrimEnd(string value, string expected)
    {
        var pathString = new PathString(value);

        var actual = pathString.TrimEnd("/login");

        actual.ToString().Should().Be(expected);
    }
}