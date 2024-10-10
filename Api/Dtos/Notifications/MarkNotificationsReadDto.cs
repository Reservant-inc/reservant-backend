namespace Reservant.Api.Dtos.Notifications;

/// <summary>
/// Request to mark notifications as read
/// </summary>
public class MarkNotificationsReadDto
{
    /// <summary>
    /// ID of the notifications to mark as read
    /// </summary>
    public required List<int> NotificationIds { get; set; }
}
