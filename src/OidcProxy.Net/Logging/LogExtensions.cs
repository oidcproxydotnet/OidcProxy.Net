using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace OidcProxy.Net.Logging;

internal static class LogExtensions
{
    public static void LogLine<T>(this ILogger<T> logger, HttpContext context, string line)
    {
        logger.Log(LogLevel.Information, "{0} [{1}] TraceId: {2} \"{3}\" \"{4}\"",
            context.GetClientIpAddress(),
            DateTime.Now, 
            context.TraceIdentifier,
            context.Request.GetEndpointAddress(),
            line);
    }
    
    public static void LogWarning<T>(this ILogger<T> logger, HttpContext context, string line)
    {
        logger.Log(LogLevel.Warning, "{0} [{1}] TraceId: {2} \"{3}\" Warning: \"{4}\"",
            context.GetClientIpAddress(),
            DateTime.Now, 
            context.TraceIdentifier,
            context.Request.GetEndpointAddress(),
            line);
    }
    
    public static void LogException<T>(this ILogger<T> logger, HttpContext context,  Exception e)
    {
        logger.Log(LogLevel.Error, "{0} [{1}] TraceId: {2} \"{3}\" responded InternalServerError: \"{4}\"",
            context.GetClientIpAddress(),
            DateTime.Now, 
            context.TraceIdentifier,
            context.Request.GetEndpointAddress(),
            e);
    }

    private static string GetEndpointAddress(this HttpRequest request)
    {
        var pathAndQuery = request.Path;
        if (request.QueryString.HasValue)
        {
            pathAndQuery += request.QueryString.Value;
        }

        return $"[{request.Method}] {pathAndQuery}";
    }

    private static string? GetClientIpAddress(this HttpContext context) => context.Connection.RemoteIpAddress?.ToString();
}