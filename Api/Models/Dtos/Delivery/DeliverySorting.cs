namespace Reservant.Api.Models.Dtos.Delivery;

/// <summary>
/// Delivery sorting options
/// </summary>
public enum DeliverySorting
{
    /// <summary>
    /// By order ascending
    /// </summary>
    OrderTimeAsc,

    /// <summary>
    /// By order time descending
    /// </summary>
    OrderTimeDesc,

    /// <summary>
    /// By time delivered ascending
    /// </summary>
    DeliveredTimeAsc,

    /// <summary>
    /// By time delivered descending
    /// </summary>
    DeliveredTimeDesc,
}
