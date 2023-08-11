namespace GoCloudNative.Bff.Authentication.OpenIdConnect.Tests.TestServer;

public class Request
{
    public Request(string url)
    {
        Url = url;
    }
    
    public string Url { get; }
}