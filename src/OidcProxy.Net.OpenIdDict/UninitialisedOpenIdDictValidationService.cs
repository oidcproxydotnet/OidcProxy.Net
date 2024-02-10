using OpenIddict.Validation;

namespace OidcProxy.Net.OpenIdDict;

internal class UninitialisedOpenIdDictValidationService : OpenIddictValidationService
{
    public UninitialisedOpenIdDictValidationService(IServiceProvider provider) : base(provider)
    {
    }
}