namespace Reservant.Api.Dtos.Notification;

/// <summary>
/// Info about a Notification
/// </summary>
public class NotificationVM
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public required int NotificationId { get; set; }

    /// <summary>
    /// When the notification was created
    /// </summary>
    public required DateTime DateCreated { get; set; }

    /// <summary>
    /// When the notification was read
    /// </summary>
    public required DateTime? DateRead { get; set; }

    /// <summary>
    /// Notification type
    /// </summary>
    public required string NotificationType { get; set; }

    /// <summary>
    /// Extra details
    /// </summary>
    /// <example>{"prop1": "string", "prop2": "string", "prop3": "string"}</example>
    public required object Details { get; set; }
}
