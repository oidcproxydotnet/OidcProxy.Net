using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.OpenIdDict;

public class DummyRedirectUri : IRedirectUriFactory
{
    public string DetermineHostName(HttpContext context)
    {
        throw new NotImplementedException();
    }

    public string DetermineRedirectUri(HttpContext context, PathString endpointName)
    {
        throw new NotImplementedException();
    }
}