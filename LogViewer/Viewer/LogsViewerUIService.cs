using LogsViewer.Data;
using LogsViewer.Logger;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;
using System.Web;

namespace LogsViewer.Viewer;

/// <summary>
/// Service responsible for rendering the logs viewer UI
/// </summary>
internal class LogsViewerUIService(LogDbContext db)
{
    /// <summary>
    /// Return the HTML of the logs viewer UI
    /// </summary>
    public async Task<string> RenderLogsPage()
    {
        var htmlBuilder = new StringBuilder();

        htmlBuilder.AppendLine("""
            <!DOCTYPE html>
            <html>
                <head>
                    <title>API Logs</title>
                    <link rel="stylesheet" href="./style.css">
                </head>
                <body>
                    <div class="container">
                        <h1>Logs</h1>
            """);

        var logs = await ReadLogs();
        foreach (var log in logs)
        {
            AddHttpRequestLog(htmlBuilder, log);
        }

        htmlBuilder.AppendLine("""
                    </div>
                </body>
            </html>
            """);

        return htmlBuilder.ToString();
    }

    private void AddHttpRequestLog(StringBuilder htmlBuilder, HttpLog log)
    {
        var requestLogHeader =
            log.TraceId.StartsWith(SqliteLoggerProvider.NonHttpTraceIdPrefix)
                ? "Non HTTP logs"
                : $"{log.Method} {log.Path ?? "[Unkown Path]"}";

        var statusClass = log.StatusCode == null ? 0 : log.StatusCode / 100 * 100;
        var statusCode = log.StatusCode?.ToString() ?? "???";
        htmlBuilder.AppendLine(
            $"""
            <details class="request-accordion">
                <summary class="request-summary">
                    <div class="status-code status-{statusClass}">{statusCode}</div>
                    <div class="request-summary-title">{requestLogHeader}</div>
                    <div class="request-summary-time">{log.StartTime:g}</div>
                </summary>
                <p>Trace ID: {HttpUtility.HtmlEncode(log.TraceId)}</p>
            """);

        foreach (var message in log.Messages)
        {
            AddLogMessage(htmlBuilder, message);
        }

        htmlBuilder.AppendLine("</details>");
    }

    private void AddLogMessage(StringBuilder htmlBuilder, LogMessage message)
    {
        htmlBuilder.AppendLine(
            $"""
            <details class="message-accordion level-{message.Level}">
                <summary>
                    <span class="message-level">{message.Level}</span>
                    {message.EventIdName}[{message.EventId}]
                </summary>
                <div class="message-content">
                    <pre>{HttpUtility.HtmlEncode(message.Message)}</pre>
                    <pre>{HttpUtility.HtmlEncode(message.Exception)}</pre>
                </div>
            </details>
            """);
    }

    private async Task<List<HttpLog>> ReadLogs()
    {
        var logs = await db.Log
            .GroupBy(l => l.TraceId ?? "")
            .Select(g => new HttpLog
            {
                TraceId = g.Key,
                Messages = g.ToList(),
                StartTime = g.Min(l => l.Timestamp)
            })
            .OrderByDescending(g => g.StartTime)
            .Take(100)
            .ToListAsync();

        foreach (var log in logs)
        {
            var requestLog = log.Messages.FirstOrDefault(m => m.EventIdName == "RequestLog");
            if (requestLog?.ParamsJson != null)
            {
                var requestData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(requestLog.ParamsJson)!;
                log.Method = requestData.GetValueOrDefault("Method").GetString();
                log.Path = requestData.GetValueOrDefault("Path").GetProperty("Value").GetString();
            }

            var responseLog = log.Messages.FirstOrDefault(m => m.EventIdName == "ResponseLog");
            if (responseLog?.ParamsJson != null)
            {
                var requestData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseLog.ParamsJson)!;
                log.StatusCode = requestData.GetValueOrDefault("StatusCode").GetInt32();
            }
        }

        return logs;
    }
}
