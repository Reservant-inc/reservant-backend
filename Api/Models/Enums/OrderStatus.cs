namespace Reservant.Api.Models.Enums;

// The order of the values matters, so that min order item status
// is the whole order's status:
// - Cancelled is the last value, so that the whole order is
//   cancelled if all the items are
// - The values are ordered by readiness

/// <summary>
/// Order item status
/// </summary>
public enum OrderStatus
{
    /// <summary>
    /// Not ready
    /// </summary>
    InProgress,

    /// <summary>
    /// Ready to be given to the client
    /// </summary>
    Ready,

    /// <summary>
    /// Received by the client
    /// </summary>
    Taken,

    /// <summary>
    /// Cancelled
    /// </summary>
    Cancelled
}
