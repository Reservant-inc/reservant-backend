namespace Reservant.Api.Dtos;

/// <summary>
/// Used to return paginated data
/// </summary>
/// <typeparam name="T">Type of one item</typeparam>
public class Pagination<T>
{
    /// <summary>
    /// Current page number (0-based)
    /// </summary>
    public required int Page { get; init; }

    /// <summary>
    /// Total number of pages
    /// </summary>
    public required int TotalPages { get; init; }

    /// <summary>
    /// Number of items on the page
    /// </summary>
    public required int PerPage { get; init; }

    /// <summary>
    /// Available orderBy options
    /// </summary>
    public required string[] OrderByOptions { get; init; }

    /// <summary>
    /// Actual item found
    /// </summary>
    public required List<T> Items { get; init; }
}
