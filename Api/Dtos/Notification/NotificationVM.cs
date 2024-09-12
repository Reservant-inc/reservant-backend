using Reservant.Api.Models.Enums;
using System.Text.Json;

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
    /// Type of the notification
    /// </summary>
    public required NotificationType NotificationType { get; set; }

    /// <summary>
    /// Extra data
    /// </summary>
    public required JsonElement Details { get; set; }
}
