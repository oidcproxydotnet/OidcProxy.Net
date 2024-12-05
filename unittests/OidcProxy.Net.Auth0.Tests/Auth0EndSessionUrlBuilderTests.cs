using System.Web;
using FluentAssertions;

namespace OidcProxy.Net.Auth0.Tests;

public class Auth0EndSessionUrlBuilderTests
{
    [InlineData(true, true, "https://foo.bar")]
    [InlineData(false, true, "https://foo.bar")]
    [InlineData(true, false, "https://foo.bar")]
    [InlineData(true, true, null)]
    [InlineData(false, true, null)]
    [Theory]
    public void ShouldStartQueryStringWithQuestionMark(bool useOidcEndpoint, bool federatedLogout, string? redirectUrl)
    {
        var expectedPrefix = new [] {
            "https://idp.test.com/oidc/logout?",
            "https://idp.test.com/v2/logout?"
        };
        
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .WithDomain("idp.test.com")
            .UseOidcLogoutEndpoint(useOidcEndpoint)
            .WithFederated(federatedLogout)
            .WithRedirectUrl(redirectUrl);
        
        var actual = auth0LogoutBuilder.Build();

        expectedPrefix.Any(expected => actual.ToString().StartsWith(expected)).Should().BeTrue();
    }

    [Fact]
    public void WhenV2_ShouldContainUrlEncodedReturnUrl()
    {
        const string expected = "https://mywebsite.com/landingpage.html";
        
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(false)
            .WithDomain("idp.test.com")
            .WithRedirectUrl(expected);
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Should().Contain($"returnTo={HttpUtility.UrlEncode(expected)}");
    }

    [Fact]
    public void WhenOidc_ShouldContainUrlEncodedPostLogoutRedirectUri()
    {
        const string expected = "https://mywebsite.com/landingpage.html";
        
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(true)
            .WithDomain("idp.test.com")
            .WithRedirectUrl(expected);
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Should().Contain($"post_logout_redirect_uri={HttpUtility.UrlEncode(expected)}");
    }
    
    [Fact]
    public void WhenOidcAndNoReturnUrl_ShouldNotContainReturnToUriParameter()
    {
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(true)
            .WithDomain("idp.test.com");
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Should().NotContain($"post_logout_redirect_uri=");
    }
    
    [Fact]
    public void WhenV2AndNoReturnUrl_ShouldNotContainPostLogoutRedirectUriParameter()
    {
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(true)
            .WithDomain("idp.test.com");
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Should().NotContain($"post_logout_redirect_uri=");
    }

    [InlineData(false, true, "https://foo.bar?p1=1&p2=2", "asdfghjkll")]
    [InlineData(true, true, "https://foo.bar?p1=1&p2=2", "asdfghjkll")]
    [Theory]
    public void WhenMultipleQueryStringValues_ShouldUseOneQuestionMarkAndTwoAmpercents(bool useOidcLogoutEndpoint,
        bool useFederated,
        string? returnUrl,
        string? idToken)
    {
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(useOidcLogoutEndpoint)
            .WithDomain("idp.test.com")
            .WithRedirectUrl(returnUrl)
            .WithFederated(useFederated)
            .WithIdTokenHint(idToken);
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Count(x => x.Equals('?')).Should().Be(1);
        actual.ToString().Count(x => x.Equals('&')).Should().Be(2);
    }
    
    [InlineData(true, false, "https://foo.bar?p1=1&p2=2", "asdfghjkll")]
    [InlineData(false, false, "https://foo.bar?p1=1&p2=2", "asdfghjkll")]
    [InlineData(true, true, null, "asdfghjkll")]
    [InlineData(false, true, null, "asdfghjkll")]
    public void WhenMultipleQueryStringValues_ShouldUseOneQuestionMarkAndOneAmpercent(bool useOidcLogoutEndpoint,
        bool useFederated,
        string? returnUrl,
        string? idToken)
    {
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(useOidcLogoutEndpoint)
            .WithDomain("idp.test.com")
            .WithRedirectUrl(returnUrl)
            .WithFederated(useFederated)
            .WithIdTokenHint(idToken);
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Count(x => x.Equals('?')).Should().Be(1);
        actual.ToString().Count(x => x.Equals('&')).Should().Be(1);
    }
    
    [InlineData(false, false, null, "asdfghjkll")]
    [InlineData(true, false, null, "asdfghjkll")]
    [InlineData(false, false, "https://foo.bar?p1=1&p2=2", null)]
    [InlineData(true, false, "https://foo.bar?p1=1&p2=2", null)]
    [Theory]
    public void WhenMultipleQueryStringValues_ShouldUseOneQuestionMarkAndNoAmpercent(bool useOidcLogoutEndpoint,
        bool useFederated,
        string? returnUrl,
        string? idToken)
    {
        var auth0LogoutBuilder = new Auth0EndSessionUrlBuilder()
            .UseOidcLogoutEndpoint(useOidcLogoutEndpoint)
            .WithDomain("idp.test.com")
            .WithRedirectUrl(returnUrl)
            .WithFederated(useFederated)
            .WithIdTokenHint(idToken);
        
        var actual = auth0LogoutBuilder.Build();

        actual.ToString().Count(x => x.Equals('?')).Should().Be(1);
        actual.ToString().Count(x => x.Equals('&')).Should().Be(0);
    }
}