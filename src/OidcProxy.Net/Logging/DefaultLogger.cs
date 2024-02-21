using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OidcProxy.Net.Logging;

internal class DefaultLogger : ILogger
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<DefaultLogger> _logger;

    public DefaultLogger(IHttpContextAccessor httpContextAccessor, ILogger<DefaultLogger> logger)
    {
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }
    
    public Task TraceAsync(string message)
    {
        Write(message, LogLevel.Trace);
        return Task.CompletedTask;
    }

    public Task InformAsync(string message)
    {
        Write(message, LogLevel.Information);
        return Task.CompletedTask;
    }

    public Task WarnAsync(string message)
    {
        Write(message, LogLevel.Warning);
        return Task.CompletedTask;
    }

    public Task WarnAsync(Exception exception)
    {
        Write(exception, LogLevel.Error);
        return Task.CompletedTask;
    }

    public Task ErrorAsync(string message)
    {
        Write(message, LogLevel.Error);
        return Task.CompletedTask;
    }

    public Task ErrorAsync(Exception exception)
    {
        Write(exception, LogLevel.Error);
        return Task.CompletedTask;
    }
    
    private void Write(string message, LogLevel logLevel)
    {
        var context = _httpContextAccessor.HttpContext;
        var logEntry = new LogEntry(context.TraceIdentifier, message);
        Write(logEntry, logLevel);
    }
    
    private void Write(Exception exception, LogLevel logLevel)
    {
        var context = _httpContextAccessor.HttpContext;
        var logEntry = new LogEntry(context.TraceIdentifier, exception);
        Write(logEntry, logLevel);
    }
    
    private void Write(LogEntry logEntry, LogLevel logLevel)
    {
        string messageFormat;
        if (logEntry.IsException)
        {
            messageFormat = $"{{@{nameof(LogEntry.Time)}}}\", " +
                            $"\"{{@{nameof(LogEntry.TraceId)}}}\", " +
                            $"\"{{@{nameof(LogEntry.Exception)}}}\" ";
            
            _logger.Log(logLevel, messageFormat, logEntry.Time, logEntry.TraceId, logEntry.Exception);
            return;
        }

        messageFormat = $"{{@{nameof(LogEntry.Time)}}}\", " +
                        $"\"{{@{nameof(LogEntry.TraceId)}}}\", " +
                        $"\"{{@{nameof(LogEntry.Message)}}}\" ";
            
        _logger.Log(logLevel, messageFormat, logEntry.Time, logEntry.TraceId, logEntry.Message);
    }
}