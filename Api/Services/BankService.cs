using Reservant.Api.Models;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service that serves as mockup of a banking app
/// </summary>
/// <param name="logger"></param>
public class BankService(Logger<LoggerFactory> logger)
{
    /// <summary>
    /// Send money to a restaurant
    /// </summary>
    /// <param name="restaurant"></param>
    /// <param name="depositAmount"></param>
    public void SendMoneyToRestaurantAsync(Restaurant restaurant, decimal? depositAmount) {
        if (depositAmount is null) {
            LoggerExtensions.BankServicePaymentNullError(logger, restaurant.Name, restaurant.RestaurantId, restaurant.Nip, DateTime.UtcNow);
        }
        LoggerExtensions.BankServicePaymentProcessingInformation(logger, depositAmount, restaurant.Name, restaurant.RestaurantId, restaurant.Nip, DateTime.UtcNow);
    }
}
