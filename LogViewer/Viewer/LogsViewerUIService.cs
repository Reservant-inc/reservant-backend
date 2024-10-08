﻿using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Web;
using Microsoft.Extensions.Logging;
using Reservant.LogsViewer.Data;
using Reservant.LogsViewer.Logger;

namespace Reservant.LogsViewer.Viewer;

/// <summary>
/// Service responsible for rendering the logs viewer UI
/// </summary>
internal class LogsViewerUIService(LogDbContext db)
{
    private const int RequestsPerPage = 100;

    /// <summary>
    /// Return the HTML of the logs viewer UI
    /// </summary>
    /// <param name="page">Logs page, starting from 1</param>
    public async Task<string> RenderLogsPage(int page)
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

        var totalPages = await CountTotalPages();
        page = Math.Clamp(page, 0, totalPages);

        AddPaging(htmlBuilder, page, totalPages);

        var logs = await ReadLogs(page);
        foreach (var log in logs)
        {
            AddHttpRequestLog(htmlBuilder, log);
        }

        AddPaging(htmlBuilder, page, totalPages);

        htmlBuilder.AppendLine("""
                    </div>

                    <script src="./site.js"></script>
                </body>
            </html>
            """);

        return htmlBuilder.ToString();
    }

    private void AddPaging(StringBuilder htmlBuilder, int page, int totalPages)
    {
        htmlBuilder.AppendLine("<div class=\"paging\">");
        if (page > 1)
        {
            htmlBuilder.AppendLine(
                $"<div><a href=\"?page={page - 1}\">Previous</a></div>");
        }
        else
        {
            htmlBuilder.AppendLine(
                $"<div class=\"disabled-link\">Previus</div>");
        }

        htmlBuilder.AppendLine($"<div>Page: {page} of {totalPages}</div>");

        if (page < totalPages)
        {
            htmlBuilder.AppendLine(
                $"<div><a href=\"?page={page + 1}\">Next</a></div>");
        }
        else
        {
            htmlBuilder.AppendLine(
                $"<div class=\"disabled-link\">Next</div>");
        }

        htmlBuilder.AppendLine("</div>");
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
            """);

        if (log.CountErrors > 0)
        {
            htmlBuilder.AppendLine(
                $"<div class=\"request-summary-bubble request-summary-bubble-errors\">{log.CountErrors}</div>");
        }
        else if (log.CountWarnings > 0)
        {
            htmlBuilder.AppendLine(
                $"<div class=\"request-summary-bubble request-summary-bubble-warnings\">{log.CountWarnings}</div>");
        }

        htmlBuilder.AppendLine(
            $"""
                    <div class="request-summary-time">
                        <time datetime="{log.StartTime:s}Z">{log.StartTime:g} UTC</time>
                    </div>
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
        var content = FormatLogMessageContent(message);
        htmlBuilder.AppendLine(
            $"""
            <details class="message-accordion level-{message.Level}">
                <summary>
                    <span class="message-level">{message.Level}</span>
                    {message.EventIdName}[{message.EventId}]
                </summary>
                <div class="message-content">
                    <pre>{HttpUtility.HtmlEncode(content)}</pre>
                    <pre>{HttpUtility.HtmlEncode(message.Exception)}</pre>
                </div>
            </details>
            """);
    }

    /// <summary>
    /// Some log messages can be formatted in a prettier way than the default formatter
    /// formats them
    /// </summary>
    private static string FormatLogMessageContent(LogMessage message)
    {
        if (message.ParamsJson is null)
        {
            return message.Message;
        }

        switch (message.EventIdName)
        {
            case "RequestBody" or "ResponseBody":
                {
                    var jsonParams = ParseJsonParams(message);

                    var bodyWasntRead = jsonParams.Prop("Status").AsString() == "[Not consumed by app]";
                    if (bodyWasntRead)
                    {
                        return "[Is not available because the app discarded it]";
                    }

                    var body = jsonParams.Prop("Body").AsString();
                    return TryPrettifyJson(body) ?? message.Message;
                }

            default:
                return message.Message;
        }
    }

    private static readonly JsonSerializerOptions PrettyJsonOptions = new()
    {
        WriteIndented = true
    };

    /// <summary>
    /// If the string contains JSON, print it idented. If not, return as is.
    /// </summary>
    private static string? TryPrettifyJson(string? json)
    {
        if (json is null)
        {
            return null;
        }

        try
        {
            var parsedResponse = JsonSerializer.Deserialize<JsonElement>(json);
            return JsonSerializer.Serialize(parsedResponse, PrettyJsonOptions);
        }
        catch (JsonException)
        {
            return json;
        }
    }

    /// <summary>
    /// Count total number of requests
    /// </summary>
    private async Task<int> CountTotalPages()
    {
        var totalRequests = await db.Log
            .Select(l => l.TraceId)
            .Distinct()
            .CountAsync();
        return (totalRequests + RequestsPerPage - 1) / RequestsPerPage;
    }

    /// <summary>
    /// Fetch logs from the database
    /// </summary>
    /// <param name="page">Page, starting from 1</param>
    private async Task<List<HttpLog>> ReadLogs(int page)
    {
        // Entity Framework does not allow selecting distinct values
        // from an ordered query, but it is possible in raw SQL
        // https://stackoverflow.com/questions/12428985/distinct-and-orderby-issue
        var logs = await db.Log
            .FromSql($"""
                SELECT *
                FROM (
                    SELECT DISTINCT TraceId AS CurrentTraceId
                    FROM Log
                    ORDER BY LogMessageId DESC
                    LIMIT {RequestsPerPage} OFFSET {(page - 1) * RequestsPerPage}
                )
                JOIN Log ON CurrentTraceId = Log.TraceId
                """)
            .GroupBy(l => l.TraceId)
            .Select(g => new HttpLog
            {
                TraceId = g.Key,
                Messages = g.OrderBy(l => l.Timestamp).ToList(),
                StartTime = g.Min(l => l.Timestamp),
                CountErrors = g.Count(l => l.Level >= LogLevel.Error),
                CountWarnings = g.Count(l => l.Level == LogLevel.Warning),
            })
            .OrderByDescending(g => g.StartTime)
            .ToListAsync();

        foreach (var log in logs)
        {
            var requestLog = log.Messages.FirstOrDefault(m => m.EventIdName == "RequestLog");
            if (requestLog?.ParamsJson != null)
            {
                var requestData = ParseJsonParams(requestLog);
                log.Method = requestData.Prop("Method").AsString();
                log.Path = requestData.Prop("Path").Prop("Value").AsString();
            }

            var responseLog = log.Messages.FirstOrDefault(m => m.EventIdName == "ResponseLog");
            if (responseLog?.ParamsJson != null)
            {
                var requestData = ParseJsonParams(responseLog);
                log.StatusCode = requestData.Prop("StatusCode").AsInt32();
            }
        }

        return logs;
    }

    /// <summary>
    /// Parse format parameters of a log message
    /// Return default(JsonElement) if null
    /// </summary>
    private static JsonElement ParseJsonParams(LogMessage message)
    {
        if (message.ParamsJson is null)
        {
            return default;
        }

        return JsonSerializer.Deserialize<JsonElement>(message.ParamsJson);
    }
}
