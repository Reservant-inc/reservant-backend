using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Models;

/// <summary>
/// Payment transaction
/// </summary>
public class PaymentTransaction
{
    /// <summary>
    /// Unique ID
    /// </summary>
    [Key]
    public int PaymentTransactionId { get; set; }

    /// <summary>
    /// Transaction title
    /// </summary>
    [StringLength(50)]
    public required string Title { get; set; }

    /// <summary>
    /// Positive for incoming money, negative for payments
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Date and time of the transaction
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// ID of the user
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Navigational property for the user
    /// </summary>
    public User User { get; set; } = null!;
}
