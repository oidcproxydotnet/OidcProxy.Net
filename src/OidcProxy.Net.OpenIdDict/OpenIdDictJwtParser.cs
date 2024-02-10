using System.IdentityModel.Tokens.Jwt;
using OidcProxy.Net.ModuleInitializers;
using OidcProxy.Net.OpenIdConnect;
using OpenIddict.Validation;

namespace OidcProxy.Net.OpenIdDict;

internal class OpenIdDictJwtParser : JwtParser
{
    private readonly OpenIddictValidationService? _validationService;

    public OpenIdDictJwtParser(ProxyOptions options, OpenIddictValidationService? validationService) 
        : base(options)
    {
        _validationService = validationService;
    }

    public override JwtPayload? ParseAccessToken(string accessToken) => null;

    public override async Task<JwtPayload?> ParseAccessTokenAsync(string accessToken)
    {
        var principal = await _validationService.ValidateAccessTokenAsync(accessToken);
        
        return new JwtPayload(principal.Claims);
    }
}