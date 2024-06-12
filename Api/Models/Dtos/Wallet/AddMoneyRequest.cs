namespace Reservant.Api.Models.Dtos.Wallet;

/// <summary>
/// Request to add money to the wallet
/// </summary>
public class AddMoneyRequest
{
    /// <summary>
    /// Amount of money to add to the wallet
    /// </summary>
    public decimal Amount { get; set; }
}
