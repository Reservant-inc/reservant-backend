﻿namespace Reservant.Api.Dtos.Deliveries;

/// <summary>
/// Basic information about a Delivery
/// </summary>
public class DeliverySummaryVM
{
    /// <summary>
    /// Delivery's unique ID
    /// </summary>
    public required int DeliveryId { get; set; }

    /// <summary>
    /// When was ordered
    /// </summary>
    public required DateTime OrderTime { get; set; }

    /// <summary>
    /// When was delivered
    /// </summary>
    public required DateTime? DeliveredTime { get; set; }

    /// <summary>
    /// When the delivery was canceled
    /// </summary>
    public required DateTime? CanceledTime { get; set; }

    /// <summary>
    /// ID of the user who received the delivery
    /// </summary>
    public required Guid? UserId { get; set; }

    /// <summary>
    /// Full name of the user who received the delivery
    /// </summary>
    public required string? UserFullName { get; set; }

    /// <summary>
    /// Total cost of the items
    /// </summary>
    public required decimal Cost { get; set; }
}
