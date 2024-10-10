using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Wallets;
using Reservant.Api.Models;
using Reservant.Api.Validation;

namespace Reservant.Api.Services;

/// <summary>
/// Service for handling transactions
/// </summary>
/// <param name="context"></param>
/// <param name="validationService"></param>
public class WalletService(
    ApiDbContext context,
    ValidationService validationService)
{
    /// <summary>
    /// creates a transaction for the specified user
    /// </summary>
    /// <param name="moneyRequest"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [ValidatorErrorCodes<PaymentTransaction>]
    public async Task<Result> CreateTransaction(AddMoneyRequest moneyRequest, User user)
    {
        var newTransaction = new PaymentTransaction
        {
            Title = moneyRequest.Title.Trim(),
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

        return Result.Success;
    }

    /// <summary>
    /// gets given user's wallet status
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<WalletStatusVM> GetWalletStatus(User user)
    {
        var balance = await context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .SumAsync(p => p.Amount);

        return new WalletStatusVM
        {
            Balance = balance
        };
    }

    /// <summary>
    /// gets the transaction history for the requested user
    /// </summary>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    [MethodErrorCodes(typeof(Utils), nameof(Utils.PaginateAsync))]
    public async Task<Result<Pagination<TransactionVM>>> GetTransactionHistory(int page, int perPage, User user)
    {
        var transactions = context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .OrderBy(p => p.Time)
            .Select(p => new TransactionVM
            {
                TransactionId = p.PaymentTransactionId,
                Title = p.Title,
                Amount = p.Amount,
                Time = p.Time,
            });

        return await transactions.PaginateAsync(page, perPage, []);

    }

}
