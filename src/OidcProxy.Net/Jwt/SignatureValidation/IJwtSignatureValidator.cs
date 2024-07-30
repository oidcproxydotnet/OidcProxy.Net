namespace OidcProxy.Net.Jwt.SignatureValidation;

public interface IJwtSignatureValidator
{
    Task<bool> Validate(string? token);
}