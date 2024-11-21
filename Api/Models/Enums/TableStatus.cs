namespace Reservant.Api.Models.Enums;

/// <summary>
/// Table Status
/// </summary>
public enum TableStatus
{
    /// <summary>
    /// There is no Visit currently
    /// </summary>
    Available,

    /// <summary>
    /// There is currently a Visit at this table
    /// </summary>
    Taken,

    /// <summary>
    /// There will be a Visit soon
    /// </summary>
    VisitSoon,
}
