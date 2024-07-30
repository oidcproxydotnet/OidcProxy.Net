using OidcProxy.Net.Jwt.SignatureValidation;
namespace OidcProxy.Net.EntraId;

internal class EntraIdJwtSignatureValidator : IJwtSignatureValidator
{
    public Task<bool> Validate(string? token)
    {
        return Task.FromResult(true); // Todo: Implement token validation for EntraId. Please consider making a pull request.
    }
}