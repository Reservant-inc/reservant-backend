using Reservant.LogsViewer.Data;

namespace Reservant.LogsViewer.Viewer;

internal class HttpLog
{
    public required string TraceId { get; set; }
    public required DateTime StartTime { get; set; }
    public required List<LogMessage> Messages { get; set; }
    public string? Method { get; set; }
    public string? Path { get; set; }
    public int? StatusCode { get; set; }
}
