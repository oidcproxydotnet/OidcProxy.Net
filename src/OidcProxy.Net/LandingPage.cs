namespace OidcProxy.Net;

internal readonly struct LandingPage
{
    private readonly string _value;
    
    public static bool TryParse(string url, out LandingPage landingPage)
    {
        landingPage = new LandingPage();
        if (string.IsNullOrEmpty(url))
        {
            return false;
        }

        if (!url.StartsWith("/") || url.StartsWith("//"))
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
}