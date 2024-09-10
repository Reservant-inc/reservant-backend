namespace Reservant.Api.Dtos.Notification;

/// <summary>
/// Information about unread notifications and messages
/// that is shown as bubbles near the buttons
/// </summary>
public class NotificationBubblesVM
{
    /// <summary>
    /// Number of unread notifications
    /// </summary>
    public required int UnreadNotificationCount { get; set; }
}
