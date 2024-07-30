namespace OidcProxy.Net.Logging;

internal class LogEntry
{
    public LogEntry(string? traceId, Exception exception)
    {
        TraceId = traceId ?? string.Empty;
        Exception = exception;
    }
    
    public LogEntry(string? traceId, string message)
    {
        TraceId = traceId ?? string.Empty;
        Message = message;
    }

    public bool IsException => string.IsNullOrEmpty(Message);
    
    public string TraceId { get; }
    public string? Message { get; }
    public Exception? Exception { get; }
    
    public DateTime Time { get; } = DateTime.Now;
}