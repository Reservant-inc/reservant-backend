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
    /// The date must be in the past
    /// </summary>
    public const string DateMustBeInPast = "DateMustBeInPast";

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
    /// Search parameters are not valid
    /// </summary>
    public const string InvalidSearchParameters = "InvalidSearchParameters";

    /// <summary>
    /// Too late to join event
    /// </summary>
    public const string UserNotInterestedInEvent = "UserNotInterestedInEvent";

    /// <summary>
    /// MustJoinUntil must be before Time
    /// </summary>
    public const string MustJoinUntilMustBeBeforeEventTime = "MustJoinUntilMustBeBeforeEventTime";

    /// <summary>
    /// The current user cannot be used in the request (for example sending a friend request
    /// </summary>
    public const string CannotBeCurrentUser = "CannotBeCurrentUser";

    /// <summary>
    /// The string can contain only letters, spaces, hyphens, apostrophes, or periods
    /// </summary>
    public const string MustBeValidName = "MustBeValidName";

    /// <summary>
    /// The string can contain only ascii letters or digits, underscores, hyphens
    /// </summary>
    public const string MustBeValidLogin = "MustBeValidLogin";

    /// <summary>
    /// The string can contain only letters, numbers, spaces, hyphens, commas, periods, or slashes.
    /// </summary>
    public const string MustBeValidAddress = "MustBeValidAddress";


    /// <summary>
    /// The string can contain only letters, numbers, or '-'.
    /// </summary>
    public const string MustBeValidCity = "MustBeValidCity";

    /// <summary>
    /// The latitude may range from -90.0 to 90.0 and longitude may range from -180.0 to 180.0.
    /// </summary>
    public const string MustBeValidCoordinates = "MustBeValidCoordinates";

    /// <summary>
    /// The users are already friends
    /// </summary>
    public const string AlreadyFriends = "AlreadyFriends";

    /// <summary>
    /// The string can contain singular plux followed by numbers
    /// </summary>
    public const string MustBeValidPhoneNumber = "MustBeValidPhoneNumber";

    /// <summary>
    /// Incorrect order of date start (or date from) and date end (or date until)
    /// </summary>
    public const string StartMustBeBeforeEnd = "StartMustBeBeforeEnd";

    /// <summary>
    /// MustJoinUntil must be before Time
    /// </summary>
    public const string EventIsFull = "EventIsFull";

    /// <summary>
    /// User is already accepted
    /// </summary>
    public const string UserAlreadyAccepted = "UserAlreadyAccepted";

    /// <summary>
    /// Event already passed
    /// </summary>
    public const string JoinDeadlinePassed = "JoinDeadlinePassed";

    /// <summary>
    /// User already rejected
    /// </summary>
    public const string UserAlreadyRejected = "UserAlreadyRejected";

    /// <summary>
    /// String must be a valid locale identifier
    /// </summary>
    public const string MustBeLocaleId = "MustBeLocaleId";

    /// <summary>
    /// Reservations can only be made for full hours or half hours
    /// </summary>
    public const string InvalidTimeSlot = "InvalidTimeSlot";

    /// <summary>
    /// The operation cannot be performed in the current state of Visit
    /// </summary>
    public const string IncorrectVisitStatus = "IncorrectVisitStatus";

    /// <summary>
    /// MenuItem is not in any active Menus
    /// </summary>
    public const string NotInAMenu = "MenuNotFound";


    /// <summary>
    /// Visit duration exceeds restaurant maximum visit time
    /// </summary>
    public const string VisitExceedsMaxTime = "VisitExceedsMaxTime";

    /// <summary>
    /// Visit duration is too short
    /// </summary>
    public const string VisitTooShort = "VisitTooShort";

    /// <summary>
    /// A reservation cannot be made as there is no available tables
    /// </summary>
    public const string NoAvailableTable = "NoAvailableTable";

    /// <summary>
    /// Order cannot be edited because all the items are either taken or cancelled
    /// </summary>
    public const string OrderIsFinished = "OrderIsFinished";

    /// <summary>
    /// There funds in the account balance doesn't allow this operation
    /// </summary>
    public const string InsufficientFunds = "InsufficientFunds";

    /// <summary>
    /// Deposit for specified visit was already made
    /// </summary>
    public const string DepositAlreadyMade = "DepositAlreadyMade";

    /// <summary>
    /// There is no deposit to be paid
    /// </summary>
    public const string NoDepositToBePaid = "NoDepositToBePaid";

    /// <summary>
    /// Opening hours of a restaurant must specify every day of the week
    /// </summary>
    public const string MustBeValidOpeningHours = "MustBeValidOpeningHours";

    /// <summary>
    /// The client has not visited a restaurant
    /// </summary>
    public const string HasNotVisitedRestaurant = "HasNotVisitedRestaurant";

    /// <summary>
    /// This visit has already started
    /// </summary>
    public const string VisitAlreadyStarted = "VisitAlreadyStarted";

    /// <summary>
    /// Order was already paid for
    /// </summary>
    public const string OrderAlreadyPaidFor = "OrderAlreadyPaidFor";

    /// <summary>
    /// Delivery has already been confirmed or canceled
    /// </summary>
    public const string DeliveryNotPending = "DeliveryNotPending";

    /// <summary>
    /// Password is incorrect
    /// </summary>
    public const string IncorrectPassword = "IncorrectPassword";


    /// <summary>
    /// The user must be an Emplyee
    /// </summary>
    public const string MustBeEmployeeId = "MustBeEmployeeId";



    /// <summary>
    /// Selected table is not available
    /// </summary>
    public const string TableNotAvailable = "TableNotAvailable";

    /// <summary>
    /// Invalid state of a visit
    /// </summary>
    public const string InvalidState = "InvalidState";

    /// <summary>
    /// Must be customer support agent
    /// </summary>
    public const string MustBeCustomerSupportAgent = "MustBeCustomerSupportAgent";
    
    /// <summary>
    /// Reporty is already resolved
    /// </summary>
    public const string AlreadyResolved = "AlreadyResolved";
    /// Must be customer support agent
    /// </summary>
    public const string InvalidOperation = "InvalidOperation";
}
