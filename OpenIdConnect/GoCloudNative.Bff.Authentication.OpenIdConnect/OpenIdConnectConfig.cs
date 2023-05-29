using System.Text.RegularExpressions;

namespace GoCloudNative.Bff.Authentication.OpenIdConnect;

public class OpenIdConnectConfig
{
    public string ClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
    
    public string Authority { get; set; } = string.Empty;

    public string DiscoveryEndpoint { get; set; } = "/.well-known/openid-configuration";

    public string[] Scopes { get; set; } = Array.Empty<string>();

    public string PostLogoutRedirectEndpoint { get; set; } = "/";

    public bool Validate(out IEnumerable<string> errors)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(ClientId))
        {
            results.Add("GCN-O-e9ba6693bb0e: Unable to start GoCloudNative.Bff. Invalid client_id. " +
                        "Configure the client_id in the appsettings.json or program.cs file and try again.");   
        }
        
        if (string.IsNullOrEmpty(ClientSecret))
        {
            results.Add("GCN-O-427413a281d9: Unable to start GoCloudNative.Bff. Invalid client_secret. " +
                        "Configure the client_secret in the appsettings.json or program.cs file and try again.");   
        }

        var urlRegex =
            @"^https?:\/\/(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~#?&\/=]*)$";
        if (Authority == null || !Regex.IsMatch(Authority, urlRegex))
        {
            results.Add("GCN-O-e0180c31edd7: Unable to start GoCloudNative.Bff. Invalid authority. " +
                        "Configure the authority in the appsettings.json or program.cs file and try again."); 
        }

        errors = results;
        return !results.Any();
    }
}