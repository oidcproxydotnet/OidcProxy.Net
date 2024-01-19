using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace OidcProxy.Net.Endpoints;

internal static class PathStringExtensions
{
    public static PathString TrimEnd(this PathString path, string pathSection)
    {
        var regex = $"{pathSection.Replace("/", @"\/")}[\\/]?$";
        
        var value = path.Value ?? string.Empty;
        
        var endpointName = Regex.Replace(value, regex, string.Empty);
        return endpointName.ToLowerInvariant();
    }

    public static PathString RemoveQueryString(this PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (value.Contains('?'))
        {
            value = value.Split('?')[0];
        }

        return value.ToLowerInvariant();
    }
}