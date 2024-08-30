using LogsViewer.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace LogsViewer.Logger;

/// <summary>
/// Logs messages to an SQLite database
/// </summary>
internal class SqliteLoggerProvider : ILoggerProvider
{
    /// <summary>
    /// Prefix used in the database for log messages that do not come from an HTTP request
    /// </summary>
    internal const string NonHttpTraceIdPrefix = "non-http:";

    /// <summary>
    /// Used to uniquely identify current application instance in the logs
    /// </summary>
    private readonly static Guid AppInstanceId = Guid.NewGuid();

    private readonly IDbContextFactory<LogDbContext> _dbContextFactory;
    private readonly IHttpContextAccessor _httpAccessor;

    public SqliteLoggerProvider(IHttpContextAccessor httpAccessor, IDbContextFactory<LogDbContext> dbContextFactory)
    {
        _httpAccessor = httpAccessor;
        _dbContextFactory = dbContextFactory;

        using var db = dbContextFactory.CreateDbContext();
        db.Database.EnsureCreated();
    }

    public void LogMessage(string? traceId, LogLevel logLevel, EventId eventId, string message, string? paramsJson, Exception? exception)
    {
        using var db = _dbContextFactory.CreateDbContext();
        db.Add(new LogMessage
        {
            TraceId = traceId ?? $"{NonHttpTraceIdPrefix}{AppInstanceId}",
            Timestamp = DateTime.UtcNow,
            Level = logLevel,
            EventId = eventId.Id,
            EventIdName = eventId.Name,
            Message = message,
            ParamsJson = paramsJson,
            Exception = exception?.ToString(),
        });
        db.SaveChanges();
    }

    /// <inheritdoc/>
    public ILogger CreateLogger(string categoryName)
    {
        return new SqliteLogger(this, _httpAccessor);
    }

    /// <inheritdoc/>
    public void Dispose() { }
}
