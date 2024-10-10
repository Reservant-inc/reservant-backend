namespace Reservant.Api.Dtos.Orders;

/// <summary>
/// Order (as in food order) sorting order (as in order by)
/// </summary>
public enum OrderSorting
{
    /// <summary>
    /// By date ascending
    /// </summary>
    DateAsc,

    /// <summary>
    /// By date descending
    /// </summary>
    DateDesc,

    /// <summary>
    /// By total cost ascending
    /// </summary>
    CostAsc,

    /// <summary>
    /// By total cost descending
    /// </summary>
    CostDesc
}
