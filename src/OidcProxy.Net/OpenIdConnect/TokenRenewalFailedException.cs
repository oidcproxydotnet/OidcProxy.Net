namespace OidcProxy.Net.OpenIdConnect;

public class TokenRenewalFailedException : ApplicationException
{
    public TokenRenewalFailedException(string errorMessage) : base(errorMessage)
    {
        
    }
}