namespace GoCloudNative.Bff.Authentication.IdentityProviders;

public class AuthorizeRequest
{
    public Uri AuthorizeUri { get; }
    public string? CodeVerifier { get; }

    public AuthorizeRequest(Uri authorizeUri)
    {
        AuthorizeUri = authorizeUri;
    }
    
    public AuthorizeRequest(Uri authorizeUri, string codeVerifier)
    {
        AuthorizeUri = authorizeUri;
        CodeVerifier = codeVerifier;
    }
}