

using Reservant.Api.Models.Dtos.MenuItem;

namespace Reservant.Api.Models.Dtos.Delivery;

/// <summary>
/// Information about delivery.
/// </summary>
public class DeliveryVM()
{
    /// <summary>
    /// Unique identifier for the delivery record.
    /// </summary>

    public required int Id { get; init; }
    
    /// <summary>
    /// Positions
    /// </summary>
    public required Tuple<MenuItemVM, int> positions { get; init; }
}