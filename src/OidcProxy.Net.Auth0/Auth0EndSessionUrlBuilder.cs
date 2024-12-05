using System.Text;
using System.Web;

namespace OidcProxy.Net.Auth0;

internal class Auth0EndSessionUrlBuilder
{
    private string _domain = string.Empty;
    private string _redirectUrl = string.Empty;
    private string _idTokenHint = string.Empty;
    private bool _useOidcLogoutEndpoint;
    private bool _federated;

    public Auth0EndSessionUrlBuilder WithDomain(string domain)
    {
        _domain = domain;
        return this;
    }
    
    public Auth0EndSessionUrlBuilder WithRedirectUrl(string redirectUrl)
    {
        // Docs on redirect URL
        // https://auth0.com/docs/authenticate/login/logout/redirect-users-after-logout
        
        _redirectUrl = redirectUrl;
        return this;
    }

    public Auth0EndSessionUrlBuilder WithIdTokenHint(string idToken)
    {
        _idTokenHint = idToken;
        return this;
    }

    public Auth0EndSessionUrlBuilder UseOidcLogoutEndpoint(bool v2EndpointEnabled)
    {
        _useOidcLogoutEndpoint = v2EndpointEnabled;
        return this;
    }

    public Auth0EndSessionUrlBuilder WithFederated(bool federated)
    {
        // Docs on federated logout:
        // https://auth0.com/docs/authenticate/login/logout/log-users-out-of-idps
        
        _federated = federated;
        return this;
    }
    
    public Uri Build()
    {
        var queryStringSeperator = "?";
        var redirectEndpoint = _useOidcLogoutEndpoint 
            ? "/oidc/logout"
            : "/v2/logout";

        var endSessionUrl = new StringBuilder("https://");
        endSessionUrl.AppendFormat("{0}{1}", _domain, redirectEndpoint);
        
        if (!string.IsNullOrEmpty(_redirectUrl))
        {
            var redirectQueryParameter = _useOidcLogoutEndpoint 
                ? "post_logout_redirect_uri"
                : "returnTo" ;
            
            endSessionUrl.AppendFormat("{0}{1}={2}", queryStringSeperator, redirectQueryParameter, HttpUtility.UrlEncode(_redirectUrl));
            queryStringSeperator = "&";
        }

        if (_useOidcLogoutEndpoint && !string.IsNullOrEmpty(_idTokenHint))
        {
            endSessionUrl.AppendFormat("{0}id_token_hint={1}", queryStringSeperator, HttpUtility.UrlEncode(_idTokenHint));
            queryStringSeperator = "&";
        }
        
        if (_federated)
        {
            endSessionUrl.AppendFormat("{0}federated", queryStringSeperator);
        }

        var raw = endSessionUrl.ToString();
        return new Uri(raw);
    }
}