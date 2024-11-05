using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;

namespace Reservant.Api.Services;

/// <summary>
/// Srevice for managing transactions
/// </summary>
/// <param name="context">database context</param>
public class TransactionService(ApiDbContext context)
{
    /// <summary>
    /// Function for making transactions
    /// </summary>
    /// <param name="user"></param>
    /// <param name="title"></param>
    /// <param name="amount"></param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.InsufficientFunds)]
    public async Task<PaymentTransaction?> MakeTransactionAsync(User user, string title, decimal amount)
    {

        var balance = await context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .SumAsync(p => p.Amount);
        if (amount < 0 && balance < amount *-1)
        {
            return null;
        }

        var newTransaction = new PaymentTransaction
        {
            Title = title,
            Amount = amount,
            Time = DateTime.UtcNow,
            UserId = user.Id,
        };

        await context.PaymentTransactions.AddAsync(newTransaction);
        await context.SaveChangesAsync();

        return newTransaction;
    }
}
