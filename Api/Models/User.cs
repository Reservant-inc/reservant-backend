using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// One class for all user classes.
/// </summary>
public class User : IdentityUser<Guid>, ISoftDeletable
{
    /// <summary>
    /// Imię.
    /// </summary>
    [ProtectedPersonalData, StringLength(30)]
    public required string FirstName { get; set; }

    /// <summary>
    /// Nazwisko.
    /// </summary>
    [ProtectedPersonalData, StringLength(30)]
    public required string LastName { get; set; }

    /// <summary>
    /// Data rejestracji.
    /// </summary>
    public required DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Data urodzenia.
    /// </summary>
    [ProtectedPersonalData]
    public DateOnly? BirthDate { get; set; }

    /// <summary>
    /// Reputacja. For Customers.
    /// </summary>
    public int? Reputation { get; set; }

    /// <summary>
    /// First name + last name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// ID of the RestaurantOwner who employs the user. For restaurant employees
    /// </summary>
    public Guid? EmployerId { get; set; }

    /// <summary>
    /// Employer of the user. For restaurant employees
    /// </summary>
    public User? Employer { get; set; }

    /// <summary>
    /// Navigational collection for the employments. For restaurant employees
    /// </summary>
    /// <returns></returns>
    public ICollection<Employment> Employments { get; set; } = null!;

    /// <summary>
    /// File name of the photo
    /// </summary>
    [StringLength(50)]
    public string? PhotoFileName { get; set; }

    /// <summary>
    /// Device token used for sending push notifications.
    /// </summary>
    /// <remarks>
    /// Ideally this should be a group token grouping all the user's devices,
    /// but for now one device will suffice.
    /// </remarks>
    [StringLength(170)]
    public string? FirebaseDeviceToken { get; set; }

    /// <summary>
    /// User's preferred language
    /// </summary>
    public CultureInfo Language { get; set; } = CultureInfo.InvariantCulture;

    /// <summary>
    /// Navigation property for the photo upload
    /// </summary>
    public FileUpload? Photo { get; set; }

    /// <summary>
    /// Navigational property for the user's file uploads
    /// </summary>
    public ICollection<FileUpload> Uploads { get; set; } = null!;

    /// <summary>
    /// Navigational property for the outgoing friend requests
    /// </summary>
    public ICollection<FriendRequest> OutgoingRequests { get; set; } = null!;

    /// <summary>
    /// Navigational property for the incoming friend requests
    /// </summary>
    public ICollection<FriendRequest> IncomingRequests { get; set; } = null!;

    /// <summary>
    /// Reviews written by the user
    /// </summary>
    public ICollection<Review> Reviews { get; set; } = null!;

    /// <summary>
    /// Events the user is interested in
    /// </summary>
    public ICollection<ParticipationRequest> EventParticipations { get; set; } = null!;

    /// <summary>
    /// Events the user has created
    /// </summary>
    public ICollection<Event> EventsCreated { get; set; } = null!;

    /// <summary>
    /// Message thread the user participates in
    /// </summary>
    public ICollection<MessageThread> Threads { get; set; } = null!;

    /// <summary>
    /// Private Message threads the user participates in
    /// </summary>
    public ICollection<PrivateMessageThread> PrivateThreads { get; set; } = null!;

    /// <summary>
    /// Notifications that the user has received
    /// </summary>
    public ICollection<Notification> Notifications { get; set; } = null!;

    /// <summary>
    /// Ingredient amount corrections
    /// </summary>
    public ICollection<IngredientAmountCorrection> IngredientCorrections { get; set; } = null!;

    /// <summary>
    /// Visits created by the user
    /// </summary>
    public ICollection<Visit> VisitsCreated { get; set; } = null!;

    /// <summary>
    /// Visits that the user participated in
    /// </summary>
    public ICollection<Visit> VisitParticipations { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Property that indicates if the user was deleted
    /// </summary>
    public bool IsArchived { get; set; }

    /// <summary>
    /// User's phone number
    /// </summary>
    [ProtectedPersonalData]
    public PhoneNumber? FullPhoneNumber { get; set; } = null!;

    /// <summary>
    /// Precise time of beeing unbanned
    /// </summary>
    public required DateTime? BannedUntil { get; set; }

    /// <summary>
    /// Check whether the user is banned at a certain point in time
    /// </summary>
    public bool IsBannedAt(DateTime pointInTime) => pointInTime < BannedUntil;
}
