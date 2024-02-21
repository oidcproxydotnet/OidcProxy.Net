namespace OidcProxy.Net.Logging;

public interface ILogger
{
    Task TraceAsync(string message);
    Task InformAsync(string message);
    Task WarnAsync(string message);
    Task WarnAsync(Exception exception);
    Task ErrorAsync(string message);
    Task ErrorAsync(Exception exception);
}