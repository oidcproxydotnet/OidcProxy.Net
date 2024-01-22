using System.Text.RegularExpressions;

namespace OidcProxy.Net;

internal readonly partial struct LandingPage
{
    private readonly string _value;
    
    public static bool TryParse(string url, out LandingPage landingPage)
    {
        landingPage = new LandingPage();
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (url == "/")
        {
            return true;
        }

        var regex = LandingPageRegex();
        if (!regex.IsMatch(url) 
            || url.Contains("://")
            || url.Contains("javascript:", StringComparison.InvariantCultureIgnoreCase)
            || url.Contains("javascript%3a", StringComparison.InvariantCultureIgnoreCase))
        {
            return false;
        }
        
        var isValidUri = Uri.TryCreate(url, UriKind.Relative, out _);
        if (!isValidUri)
        {
            return false;
        }
        
        landingPage = new LandingPage(url);
        return true;
    }
    
    public LandingPage()
    {
        _value = "/";
    }

    private LandingPage(string landingPage)
    {
        _value = landingPage;
    }

    public override string ToString() => string.IsNullOrEmpty(_value) ? "/" : _value;

    public bool Equals(string url) => this.ToString().Equals(url, StringComparison.InvariantCultureIgnoreCase);

    [GeneratedRegex("^\\/[a-zA-Z0-9.]")]
    private static partial Regex LandingPageRegex();
}