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

    /// <summary>
    /// The user is already currently employed at the restaurant
    /// </summary>
    public const string EmployeeAlreadyEmployed = "EmployeeAlreadyEmployed";

    /// <summary>
    /// The user must be current user's employee
    /// </summary>
    public const string MustBeCurrentUsersEmployee = "MustBeCurrentUsersEmployee";

    /// <summary>
    /// Must be a valid customer ID
    /// </summary>
    public const string MustBeCustomerId = "MustBeCustomerId";

    /// <summary>
    /// The date must be in the future
    /// </summary>
    public const string DateMustBeInFuture = "DateMustBeInFuture";

    /// <summary>
    /// Table does not exist
    /// </summary>
    public const string TableDoesNotExist = "TableDoesNotExist";

    /// <summary>
    /// NumberOfGuests must be greater of equal to 0
    /// </summary>
    public const string NumberOfGuests = "NumberOfGuests";

    /// <summary>
    /// NumberOfGuests must be greater of equal to 0
    /// </summary>
    public const string RestaurantDoesNotExist = "RestaurantDoesNotExist";

    /// <summary>
    /// Tip cannot be a negative value
    /// </summary>
    public const string Tip = "Tip";

    /// <summary>
    /// The value must be greater than or equal to zero.
    /// </summary>
    public const string ValueLessThanZero = "ValueLessThanZero";

    /// <summary>
    /// to access this value you need to be employed by specific restaurant
    /// </summary>
    public const string MustBeRestaurantEmployee = "MustBeRestaurantEmployee";

    /// <summary>
    /// The value must be greater than or equal to one.
    /// </summary>
    public const string ValueLessThanOne = "ValueLessThanOne";

    /// <summary>
    /// Per page must be at least 1 and less than a certain value
    /// </summary>
    public const string InvalidPerPageValue = "InvalidPerPageValue";

    /// <summary>
    /// The order cannot be cancelled because some of the items have been received by the customer
    /// </summary>
    public const string SomeOfItemsAreTaken = "SomeOfItemsWereTaken";

    /// <summary>
    /// Error returned by Identity Framework
    /// </summary>
    public const string IdentityError = "IdentityError";

    /// <summary>
    /// Uploaded file is too big
    /// </summary>
    public const string FileTooBig = "FileTooBig";

    /// <summary>
    /// Upload content type is not accepted
    /// </summary>
    public const string UnacceptedContentType = "UnacceptedContentType";

    /// <summary>
    /// An object does not belong to the current restaurant
    /// </summary>
    public const string BelongsToAnotherRestaurant = "BelongsToAnotherRestaurant";

    /// <summary>
    /// List cannot be empty
    /// </summary>
    public const string EmptyList = "EmptyList";

    /// <summary>
    /// User is not the creator nor a participant of the visit
    /// </summary>
    public const string UserDoesNotParticipateInVisit = "UserDoesNotParticipateInVisit";

    /// <summary>
    /// Same object already exists
    /// </summary>
    public const string Duplicate = "Duplicate";

    /// <summary>
    /// Value exceeds limit
    /// </summary>
    public const string ValueExceedsLimit = "ValueExceedsLimit";

    /// <summary>
    /// Wrong polygon format
    /// </summary>
    public const string WrongPolygonFormat = "WrongPolygonFormat";

    /// <summary>
    /// Too late to join event
    /// </summary>
    public const string UserNotInterestedInEvent = "UserNotInterestedInEvent";
}
