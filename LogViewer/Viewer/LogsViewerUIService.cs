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
                </head>
                <body>
                    <h1>Logs</h1>
            """);

        var logs = await ReadLogs();
        foreach (var log in logs)
        {
            AddHttpRequestLog(htmlBuilder, log);
        }

        htmlBuilder.AppendLine("""
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
                : $"{log.Method} {log.Path ?? "[Unkown Path]"} => {log.StatusCode?.ToString() ?? "[No Response]"}";

        htmlBuilder.AppendLine(
            $"""
            <details>
                <summary style="font-size: 1.5rem; font-weight: bold;">[{log.StartTime:g}] {requestLogHeader}</summary>
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
        var color = GetColorForLogLevel(message.Level);

        htmlBuilder.AppendLine(
            $"""
            <details>
                <summary style="background-color:{color};">
                    <span style="background-color:rgba(0,0,30,0.2)">{message.Level}</span> {message.EventIdName}[{message.EventId}]
                </summary>
                <p><pre>{HttpUtility.HtmlEncode(message.Message)}</pre></p>
                <p><pre>{HttpUtility.HtmlEncode(message.Exception)}</pre></p>
            </details>
            """);
    }

    private static string GetColorForLogLevel(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => "#bbffbb",
            LogLevel.Debug => "#bbbbbb",
            LogLevel.Information => "#bbbbff",
            LogLevel.Warning => "#ffffbb",
            LogLevel.Error => "#ffbbbb",
            LogLevel.Critical => "#ff0000",
            _ => "none",
        };
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
