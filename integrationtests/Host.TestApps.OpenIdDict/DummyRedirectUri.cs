using OidcProxy.Net.OpenIdConnect;

namespace Host.TestApps.OpenIdDict;

public class DummyRedirectUri : IRedirectUriFactory
{
    public string DetermineHostName()
    {
        throw new NotImplementedException();
    }

    public string DetermineRedirectUri(PathString endpointName)
    {
        throw new NotImplementedException();
    }
}