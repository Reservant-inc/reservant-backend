using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Wallet;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

public class WalletService(
    ApiDbContext context,
    ValidationService validationService)
{

    public async Task<Result<PaymentTransaction>> CreateTransaction(AddMoneyRequest moneyRequest, User user)
    {
        var newTransaction = new PaymentTransaction
        {
            Title = "?",
            Amount = moneyRequest.Amount,
            Time = DateTime.UtcNow,
            UserId = user.Id,
        };

        var result = await validationService.ValidateAsync(newTransaction, user.Id);
        if (!result.IsValid)
        {
            return result;
        }

        context.PaymentTransactions.Add(newTransaction);
        await context.SaveChangesAsync();

        return newTransaction;
    }

}
