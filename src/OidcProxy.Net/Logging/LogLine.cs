using Microsoft.Extensions.Logging;

namespace OidcProxy.Net;

internal class LogLine
{
    public LogLine()
    {
        
    }

    public LogLine(string response)
    {
        Response = response;
    }
    
    public LogLevel Severity { get; set; } = LogLevel.Information;

    public string Response { get; } = string.Empty;
}