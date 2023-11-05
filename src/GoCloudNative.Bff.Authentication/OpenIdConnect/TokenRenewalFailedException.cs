namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public class TokenRenewalFailedException : ApplicationException
{
    public TokenRenewalFailedException(string errorMessage) : base(errorMessage)
    {
        
    }
}