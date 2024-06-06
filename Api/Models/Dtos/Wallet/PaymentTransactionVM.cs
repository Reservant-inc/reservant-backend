namespace Reservant.Api.Models.Dtos.Wallet;

/// <summary>
/// Information about a payment transaction
/// </summary>
public class TransactionVM
{
    /// <summary>
    /// Transaction title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Positive for incoming money, negative for payments
    /// </summary>
    public decimal Amount { get; set; }

    /// <summary>
    /// Date and time of the transaction
    /// </summary>
    public DateTime Time { get; set; }
}
