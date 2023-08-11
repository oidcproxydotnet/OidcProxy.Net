using Microsoft.Extensions.Logging;

namespace GoCloudNative.Bff.Authentication;

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