using LogsViewer.Data;
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

        var logs = await db.Log
            .GroupBy(l => l.TraceId ?? "")
            .Select(g => new { g.Key, Messages = g.ToList(), Timestamp = g.Min(l => l.Timestamp) })
            .OrderByDescending(g => g.Timestamp)
            .Take(100)
            .ToListAsync();

        foreach (var grouping in logs)
        {
            AddHttpRequestLog(htmlBuilder, grouping.Key, grouping.Messages);
        }

        htmlBuilder.AppendLine("""
                </body>
            </html>
            """);

        return htmlBuilder.ToString();
    }

    private void AddHttpRequestLog(StringBuilder htmlBuilder, string traceId, List<LogMessage> messages)
    {
        string? method = null;
        string? path = null;
        int? responseCode = null;

        var requestLog = messages.FirstOrDefault(m => m.EventIdName == "RequestLog");
        if (requestLog?.ParamsJson != null)
        {
            var requestData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(requestLog.ParamsJson)!;
            method = requestData.GetValueOrDefault("Method").GetString();
            path = requestData.GetValueOrDefault("Path").GetProperty("Value").GetString();
        }

        var responseLog = messages.FirstOrDefault(m => m.EventIdName == "ResponseLog");
        if (responseLog?.ParamsJson != null)
        {
            var requestData = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(responseLog.ParamsJson)!;
            responseCode = requestData.GetValueOrDefault("StatusCode").GetInt32();
        }

        var requestLogHeader =
            traceId == ""
                ? "Non HTTP logs"
                : $"{method} {path ?? "[Unkown Path]"} => {responseCode?.ToString() ?? "[No Response]"}";

        htmlBuilder.AppendLine(
            $"""
            <details>
                <summary style="font-size: 1.5rem; font-weight: bold;">{requestLogHeader}</summary>
                <p>Trace ID: {HttpUtility.HtmlEncode(traceId)}</p>
            """);

        foreach (var message in messages)
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
}
