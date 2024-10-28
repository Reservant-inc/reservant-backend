namespace Reservant.Api.Models.Enums;

/// <summary>
/// Enum that specifies the order by which the search shall commence.
/// </summary>
public enum SearchOrder
{
    /// <summary>
    /// Value for sorting from the oldest to the most new
    /// </summary>
    DateCreatedAsc,
    /// <summary>
    /// Value for sorting from the most new to the oldest
    /// </summary>
    DateCreatedDesc,
    /// <summary>
    /// Value for sorting from the oldest execution date to the most new
    /// </summary>
    DateAsc,
    /// <summary>
    /// Value for sorting from the most new execution date to the oldest
    /// </summary>
    DateDesc
}
