namespace GoCloudNative.Bff.Authentication;

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
        
        try
        {
            _ = new Uri(url, UriKind.Relative);
            landingPage = new LandingPage(url);
            return true;
        }
        catch (Exception)
        {
            return false;
        }
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