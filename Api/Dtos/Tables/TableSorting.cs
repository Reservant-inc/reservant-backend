namespace Reservant.Api.Dtos.Tables;

/// <summary>
/// Sorting options for tables
/// </summary>
public enum TableSorting
{
    /// <summary>
    /// By status, available first
    /// </summary>
    StatusAsc,
    
    /// <summary>
    /// By status, taken first
    /// </summary>
    StatusDesc
}