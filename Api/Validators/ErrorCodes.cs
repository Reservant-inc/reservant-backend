namespace Reservant.Api.Validators;

/// <summary>
/// Error codes that can be returned by validators
/// </summary>
public static class ErrorCodes
{
    /// <summary>
    /// Entity not found
    /// </summary>
    public const string NotFound = "NotFound";

    /// <summary>
    /// User cannot use the entity (for example they do not own it)
    /// </summary>
    public const string AccessDenied = "AccessDenied";

    /// <summary>
    /// Must be a valid restaurant tag (List of restaurant tags: GET /restaurant-tags)
    /// </summary>
    public const string RestaurantTag = "RestaurantTag";

    /// <summary>
    /// Must be a valid file upload name. WARNING! The whole error code
    /// will be `FileName.{file class}` (e.g. `FileName.Document`, `FileName.Image`)
    /// </summary>
    public const string FileName = "FileName";

    /// <summary>
    /// Must be a valid <a href="https://pl.wikipedia.org/wiki/Numer_identyfikacji_podatkowej">NIP</a>
    /// </summary>
    public const string Nip = "Nip";

    /// <summary>
    /// Must be a valid postal code (e.g. 00-000)
    /// </summary>
    public const string PostalCode = "PostalCode";

    /// <summary>
    /// Employee needs at least one role selected
    /// </summary>
    public const string AtLeastOneRoleSelected = "AtLeastOneRoleSelected";

    public const string EmployeeAlreadyEmployed = "EmployeeAlreadyEmployed";

    /// <summary>
    /// The user must be current user's employee
    /// </summary>
    public const string MustBeCurrentUsersEmployee = "MustBeCurrentUsersEmployee";
    
    /// <summary>
    /// Item list cannot be empty
    /// </summary>
    public const string EmptyItemList = "EmptyItemList";
    
    /// <summary>
    /// OrderItem does not exists
    /// </summary>
    public const string OrderItemDoesNotExists = "OrderItemDoesNotExists";
    
    /// <summary>
    /// VisitId cannot be negative value
    /// </summary>
    public const string VisitId = "VisitId";
}
