namespace OidcProxy.Net.Logging;

internal class LogEntry
{
    public LogEntry(string traceId, Exception exception)
    {
        TraceId = traceId;
        Exception = exception;
    }
    
    public LogEntry(string traceId, string message)
    {
        TraceId = traceId;
        Message = message;
    }

    public bool IsException => string.IsNullOrEmpty(Message);
    
    public string TraceId { get; }
    public string? Message { get; }
    public Exception? Exception { get; }
    
    public DateTime Time { get; } = DateTime.Now;
}