namespace Reservant.Api.Models.Dtos.Transaction;

/// <summary>
/// viewmodel of transaction class
/// </summary>
public class TransactionVM
{
    /// <summary>
    /// id of the transaction
    /// </summary>
    public required int TransactionId { get; set; }

    /// <summary>
    /// amount of money in the transaction
    /// </summary>
    public required double Amount { get; set; }

    /// <summary>
    /// date of the transaction
    /// </summary>
    public required DateOnly Date { get; set; }
}
