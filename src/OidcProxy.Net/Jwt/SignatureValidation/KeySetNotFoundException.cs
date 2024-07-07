namespace OidcProxy.Net.Jwt.SignatureValidation;

public class KeySetNotFoundException : ApplicationException
{
    public KeySetNotFoundException(string message) : base(message)
    {
        
    }
}