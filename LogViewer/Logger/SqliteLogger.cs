using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace Reservant.LogsViewer.Logger;

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
        provider.LogMessage(
            httpAccessor.HttpContext?.TraceIdentifier,
            logLevel,
            eventId,
            formatter(state, exception),
            SerializeState(state),
            exception);
    }

    /// <summary>
    /// Convert log state into a JSON string to be stored in the database
    /// </summary>
    /// <remarks>
    /// Return null if cannot serialize.
    /// 
    /// Unnamed format parameters get named after their index in the format string.
    /// </remarks>
    private static string? SerializeState(object? state)
    {
        if (state is not IEnumerable<KeyValuePair<string, object>> statePairs)
        {
            return null;
        }

        var paramsDictionary = new Dictionary<string, object>();
        var index = 0;
        foreach (var pair in statePairs)
        {
            var key = string.IsNullOrEmpty(pair.Key) ? index.ToString() : pair.Key;
            paramsDictionary.Add(key, pair.Value);

            index++;
        }

        try
        {
            return Encoding.UTF8.GetString(JsonSerializer.SerializeToUtf8Bytes(paramsDictionary));
        }
        catch (NotSupportedException)
        {
            return null;
        }
    }
}
