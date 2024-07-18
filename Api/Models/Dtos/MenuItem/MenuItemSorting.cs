namespace Reservant.Api.Models.Dtos.MenuItem;

/// <summary>
/// Menu item sorting order
/// </summary>
public enum MenuItemSorting
{
    /// <summary>
    /// By name ascending
    /// </summary>
    NameAsc,

    /// <summary>
    /// By name descending
    /// </summary>
    NameDesc,

    /// <summary>
    /// By price ascending
    /// </summary>
    PriceAsc,

    /// <summary>
    /// By price descending
    /// </summary>
    PriceDesc,

    /// <summary>
    /// By alcohol percentage ascending
    /// </summary>
    AlcoholAsc,

    /// <summary>
    /// By alcohol percentage descending
    /// </summary>
    AlcoholDesc
}
