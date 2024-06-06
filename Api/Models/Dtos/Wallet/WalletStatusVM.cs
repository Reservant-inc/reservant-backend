namespace Reservant.Api.Models.Dtos.Wallet;

/// <summary>
/// Information about wallet status
/// </summary>
public class WalletStatusVM
{
    /// <summary>
    /// Current amount of money
    /// </summary>
    public decimal Balance { get; set; }
}
