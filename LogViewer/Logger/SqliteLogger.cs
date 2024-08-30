using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace LogsViewer.Logger;

/// <summary>
/// Logs messages to an SQLite database
/// </summary>
internal class SqliteLogger(SqliteLoggerProvider provider, IHttpContextAccessor httpAccessor) : ILogger
{
    /// <inheritdoc/>
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    /// <inheritdoc/>
    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Information;
    }

    /// <inheritdoc/>
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var paramsDictionary = (state as IEnumerable<KeyValuePair<string, object>>)?.ToDictionary(i => i.Key, i => i.Value);

        string? paramsJson = null;
        if (paramsDictionary is not null)
        {
            try
            {
                paramsJson = Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(paramsDictionary));
            }
            catch (NotSupportedException) { }
        }
        
        provider.LogMessage(
            httpAccessor.HttpContext?.TraceIdentifier,
            logLevel,
            eventId,
            formatter(state, exception),
            paramsJson,
            exception);
    }
}
