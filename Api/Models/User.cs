using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;
using Reservant.Api.Data;

namespace Reservant.Api.Models;

/// <summary>
/// One class for all user classes.
/// </summary>
public class User : IdentityUser, ISoftDeletable
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
    [StringLength(36)]
    public string? EmployerId { get; set; }

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
    /// Notifications that the user has received
    /// </summary>
    public ICollection<Notification> Notifications { get; set; } = null!;

    /// <inheritdoc />
    public bool IsDeleted { get; set; }

    /// <summary>
    /// Property that indicates if the user was deleted
    /// </summary>
    public bool IsArchived { get; set; } = false;
}
