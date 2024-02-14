namespace OidcProxy.Net.OpenIdConnect;

public class Scopes : List<string>
{
    public Scopes(IEnumerable<string> scopes)
    {
        AddRange(scopes.Select(x => x.ToLowerInvariant()));

        const string openId = "openid";
        if (!Contains(openId))
        {
            Add(openId);
        }
        
        const string offlineAccessScope = "offline_access";
        if (!Contains(offlineAccessScope))
        {
            Add(offlineAccessScope);
        }
    }
}