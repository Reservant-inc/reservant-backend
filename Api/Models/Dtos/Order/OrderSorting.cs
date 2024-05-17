namespace Reservant.Api.Models.Dtos.Order;

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
    CostDesc,

    /// <summary>
    /// By employee's full name ascending
    /// </summary>
    EmployeeFullNameAsc,

    /// <summary>
    /// By employee's full name descending
    /// </summary>
    EmployeeFullNameDesc
}
