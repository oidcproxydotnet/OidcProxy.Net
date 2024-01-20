using System.Text.RegularExpressions;

namespace OidcProxy.Net.Auth0;

public class  Auth0Config
{
    public string ClientId { get; set; } = string.Empty;
    
    public string ClientSecret { get; set; } = string.Empty;
    
    public string Domain { get; set; } = string.Empty;
    
    public string Audience { get; set; } = string.Empty;

    public string[] Scopes { get; set; } = Array.Empty<string>();

    public bool FederatedLogout { get; set; } = false;

    public bool Validate(out IEnumerable<string> errors)
    {
        var results = new List<string>();
        if (string.IsNullOrEmpty(ClientId))
        {
            results.Add($"GCN-O-e9ba6693bb0e: Unable to start OidcProxy.Net. \"{ClientId}\" is not a valid client_id. " +
                        "Configure the client_id in the appsettings.json or program.cs file and try again. " +
                        "More info: https://bff.gocloudnative.org/errors/gcn-o-e9ba6693bb0e");   
        }
        
        if (string.IsNullOrEmpty(ClientSecret))
        {
            results.Add($"GCN-O-427413a281d9: Unable to start OidcProxy.Net. \"{ClientSecret}\" is not a valid client_secret. " +
                        "Configure the client_secret in the appsettings.json or program.cs file and try again. " +
                        "More info: https://bff.gocloudnative.org/errors/gcn-o-427413a281d9");   
        }
        
        if (string.IsNullOrEmpty(Audience))
        {
            results.Add($"GCN-A-fe95cf8c11ae: Unable to start OidcProxy.Net. \"{Audience}\" is not a valid audience. " +
                        "Configure the audience in the appsettings.json or program.cs file and try again. " +
                        "More info: https://bff.gocloudnative.org/errors/gcn-a-fe95cf8c11ae");   
        }
        
        var urlRegex =
            @"^(?:www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b(?:[-a-zA-Z0-9()@:%_\+.~]*)$";
        if (Domain == null || !Regex.IsMatch(Domain, urlRegex))
        {
            results.Add($"GCN-A-1701a00d8c56: Unable to start OidcProxy.Net. \"{Domain}\" is not a valid Auth0 Domain. " +
                        "Configure the domain in the appsettings.json or program.cs file and try again. " +
                        "More info: https://bff.gocloudnative.org/errors/gcn-a-1701a00d8c56"); 
        }

        errors = results;
        return !results.Any();
    }
}