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
    /// Must be a valid participants list
    /// </summary>
    public const string ParticipantDoesNotExist = "ParticipantDoesNotExist";
    
    /// <summary>
    /// The date must be in the future
    /// </summary>
    public const string DateMustBeInFuture = "DateMustBeInFuture";
    
    /// <summary>
    /// Table does not exist
    /// </summary>
    public const string TableDoesNotExists = "TableDoesNotExists";

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
}
