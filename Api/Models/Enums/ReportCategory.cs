namespace Reservant.Api.Models.Enums;

/// <summary>
/// Category of a user report
/// </summary>
public enum ReportCategory
{
    /// <summary>
    /// Reporting a problem with the application
    /// </summary>
    Technical,

    /// <summary>
    /// Reporting a lost item
    /// </summary>
    LostItem,

    /// <summary>
    /// Reporting a restaurant employee
    /// </summary>
    RestaurantEmployeeReport,

    /// <summary>
    /// Reporting a customer
    /// </summary>
    CustomerReport,
}
