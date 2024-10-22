namespace Reservant.Api;

/// <summary>
/// ILogger extensions for optimized logging
/// </summary>
public static partial class LoggerExtensions
{
    /// <summary>
    /// An example upload was added by the database seeder
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, EventId = 1000, EventName = "Reservant.Api.SeedUpload",
        Message = "Added example upload {FileName} for user {UserLogin} (Id: {UserId})")]
    public static partial void ExampleUploadAdded(
        this ILogger logger, string fileName, string userLogin, Guid userId);

    /// <summary>
    /// Web Socket client connection closed prematurely
    /// </summary>
    [LoggerMessage(Level = LogLevel.Information, EventId = 1001, EventName = "Reservant.Api.WsClosedPrematurely",
        Message = "Web Socket client connection closed prematurely")]
    public static partial void WebSocketConnectionClosedPrematurely(this ILogger logger, Exception ex);

    /// <summary>
    /// Unexpected error happened in a Web Socket connection
    /// </summary>
    [LoggerMessage(Level = LogLevel.Error, EventId = 1002, EventName = "Reservant.Api.WsUnexpectedError",
        Message = "Unexpected WebSocket error occured")]
    public static partial void WebSocketUnexpectedError(this ILogger logger, Exception ex);

    /// <summary>
    /// Failed to send a Firebase push notification
    /// </summary>
    [LoggerMessage(Level = LogLevel.Warning, EventId = 1003, EventName = "Reservant.Api.FirebaseMessagingError",
        Message = "Failed to send a Firebase push notification to user ID {UserId}")]
    public static partial void FirebaseMessagingError(this ILogger logger, Exception ex, Guid userId);
}
