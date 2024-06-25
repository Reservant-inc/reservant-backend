namespace Reservant.Api.Models.Dtos.Wallet;

/// <summary>
/// Information about wallet status
/// </summary>
public class WalletStatusVM
{
    /// <summary>
    /// Current amount of money
    /// </summary>
    public required decimal Balance { get; set; }
}
