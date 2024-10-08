﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Reservant.LogsViewer.Data;

/// <summary>
/// Represents a log message
/// </summary>
[Index(nameof(TraceId))]
internal class LogMessage
{
    public int LogMessageId { get; set; }
    public string TraceId { get; set; } = null!;
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public int EventId { get; set; }
    public string? EventIdName { get; set; }
    public string Message { get; set; } = null!;
    public string? ParamsJson { get; set; } = null!;
    public string? Exception { get; set; } = null!;
}
