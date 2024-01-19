namespace OidcProxy.Net;

internal readonly struct ErrorPage
{
    private readonly string _value;
    
    public static bool TryParse(string url, out ErrorPage errorPage)
    {
        errorPage = new ErrorPage();
        
        if (!LandingPage.TryParse(url, out _))
        {
            return false;
        }

        if (url.Contains('?') || url.Contains('#'))
        {
            return false;
        }

        errorPage = new ErrorPage(url);
        return true;
    }
    
    public ErrorPage()
    {
        _value = "/";
    }

    private ErrorPage(string landingPage)
    {
        _value = landingPage;
    }

    public override string ToString() => _value;
}