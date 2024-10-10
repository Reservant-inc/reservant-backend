namespace Reservant.Api.Dtos.Reviews;

/// <summary>
/// Order (as in food order) sorting order (as in order by)
/// </summary>
public enum ReviewOrderSorting
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
    /// By Stars ascending
    /// </summary>
    StarsAsc,

    /// <summary>
    /// By Stars descending
    /// </summary>
    StarsDesc
}
