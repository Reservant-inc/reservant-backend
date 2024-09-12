
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
    /// Request date
    /// </summary>
    public DateTime RequestDate { get; set; }
    
    /// <summary>
    /// Boolean for accept
    /// </summary>
    public bool IsAccepted { get; set; }
    
    /// <summary>
    /// Boolean for reject
    /// </summary>
    public bool IsRejected { get; set; }
}