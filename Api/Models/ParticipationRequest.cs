
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Participation request
/// </summary>
public class ParticipationRequest
{
    /// <summary>
    /// Unique ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Event ID
    /// </summary>
    public int EventId { get; set; }

    /// <summary>
    /// Event navigation property
    /// </summary>
    public Event Event { get; set; }

    /// <summary>
    /// User ID
    /// </summary>
    [StringLength(36)]
    public string UserId { get; set; }

    /// <summary>
    /// User navigation property
    /// </summary>
    public User User { get; set; }

    /// <summary>
    /// Time when the request was sent
    /// </summary>
    public DateTime DateSent { get; set; }

    /// <summary>
    /// Time when user got accepted
    /// </summary>
    public DateTime? DateAccepted { get; set; }

    /// <summary>
    /// Time when the request was deleted or rejected (same thing)
    /// </summary>
    public DateTime? DateDeleted { get; set; }
}
