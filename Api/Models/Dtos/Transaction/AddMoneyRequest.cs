namespace Reservant.Api.Models.Dtos.Transaction;


/// <summary>
/// dto for creating transactions for users
/// </summary>
public class AddMoneyRequest
{

    /// <summary>
    /// user who makes the transaction
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// amount of money in the transaction
    /// </summary>
    public required double Amount { get; set; }
}
