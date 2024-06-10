namespace Reservant.Api.Models.Dtos.Wallet;

/// <summary>
/// Request to add money to the wallet
/// </summary>
public class AddMoneyRequest
{

    /// <summary>
    /// Transaction title
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// Amount of money to add to the wallet
    /// </summary>
    public decimal Amount { get; set; }
}
