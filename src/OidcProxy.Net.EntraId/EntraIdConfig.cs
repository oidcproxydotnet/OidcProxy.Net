using OidcProxy.Net.OpenIdConnect;

namespace OidcProxy.Net.EntraId;

public class EntraIdConfig : OpenIdConnectConfig
{
    public string TenantId { get; set; } = string.Empty;
    
    public EntraIdConfig()
    {
        this.DiscoveryEndpoint = $"{Authority}/v2.0/.well-known/openid-configuration";
    }

    public override bool Validate(out IEnumerable<string> errors)
    {
        base.Validate(out var errorMessages);
        
        errorMessages = errorMessages
            .Where(x => !x.Contains("GCN-O-e0180c31edd7"));

        var isValid = !errorMessages.Any();

        var results = errorMessages
            .Select(x => x.Replace("-O-", "-AZ-"))
            .Select(x => x.Replace("-o-", "-az-"))
            .ToList();
        
        if (string.IsNullOrEmpty(TenantId))
        {
            isValid = false;
            results.Add("GCN-AZ-42d458c58299: Unable to start OidcProxy.Net. Invalid TenantId. " +
                        "Configure the TenantId in the appsettings.json or program.cs file and try again. " +
                        "More info: https://github.com/oidcproxydotnet/OidcProxy.Net/wiki/Errors#gcn-az-42d458c58299");   
        }

        errors = results;
        return isValid;
    }
}