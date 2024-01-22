using FluentAssertions;

namespace OidcProxy.Net.Tests.UnitTests;

public class SessionExtensionTests
{
    [InlineData("/test.aspx")]
    [InlineData("/hello")]
    [Theory]
    public async Task ShouldStoreLandingPageInSession(string landingPage)
    {
        var testSession = new TestSession();

        await testSession.SetUserPreferredLandingPageAsync(landingPage);
        var actual = testSession.GetUserPreferredLandingPage();

        actual.Should().Be(landingPage);
    }
    
    [Fact]
    public async Task WhenEmptyLandingPage_ShouldRemoveLandingpageValueFromSession()
    {
        var testSession = new TestSession();

        await testSession.SetUserPreferredLandingPageAsync(string.Empty);
        var actual = testSession.GetUserPreferredLandingPage();

        actual.Should().BeNull();
    }
    
    [InlineData("https://www.myhackersite.com/youregonnabepwned.html")]
    [InlineData("http://www.myhackersite.com/youregonnabepwned.html")]
    [InlineData("//www.myhackersite.com/youregonnabepwned.html")]
    [InlineData("https://www.myhackersite.com")]
    [InlineData("zoommtg://zoom.us/join?action=join&confno=123456789&zc=64&confid=dXRpZDmMTZl&browser=chrome'")]
    [InlineData("admin:/usr/var/foo.txt")]
    [InlineData("app://visualstudio")]
    [InlineData("freeplane:/%20yadayada#ID_25")]
    [InlineData("javascript:alert('pwnage')")]
    [InlineData("jdbc:somejdbcvendor:other_data")]
    [InlineData("psns://browse?product=325256")]
    [InlineData("rdar://10198949")]
    [InlineData("s3://bucket/")]
    [InlineData("slack://open?team=owasp")]
    [InlineData("stratum+tcp://server:25")]
    [InlineData("viber://pa?chatURI=xyz")]
    [InlineData("web+lowercasealphabeticalcharacters")]
    [InlineData(@"\\C:\youre\pwned\maliciousexecutable.exe")]
    [InlineData(@"file://page.html")]
    [Theory]
    public async Task WhenMaliciousRequest_ShouldThrowException(string baaaddddd)
    {
        var testSession = new TestSession();

        Func<Task> actual = async () => await testSession.SetUserPreferredLandingPageAsync(baaaddddd);

        await actual.Should().ThrowAsync<NotSupportedException>();
    }
}