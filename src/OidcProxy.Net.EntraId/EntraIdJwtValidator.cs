using OidcProxy.Net.Jwt.SignatureValidation;
namespace OidcProxy.Net.EntraId;

internal class EntraIdJwtSignatureValidator : IJwtSignatureValidator
{
    public async Task<bool> Validate(string token)
    {
        return true; // Todo: Implement token validation for EntraId. Please consider making a pull request.
    }
}