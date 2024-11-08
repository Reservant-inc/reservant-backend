using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Wallets;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using FluentValidation.Results;
using Reservant.Api.Validators;
using Microsoft.AspNetCore.Mvc;

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

    /// <summary>
    /// Make deposit for a reservation that is connected to the specified visit
    /// </summary>
    /// <param name="user">user that created the reservation</param>
    /// <param name="title">title of the deposit transaction</param>
    /// <param name="amount">amount to pay for the deposit transaction</param>
    /// <returns>DTO with confirmation of the payment</returns>
    [ErrorCode(null, ErrorCodes.InsufficientFunds)]
    [MethodErrorCodes<WalletService>(nameof(GetWalletStatus))]
    public async Task<Result<TransactionVM>> DebitAsync(User user, string title, decimal amount)
    {
        //check for sufficient funds to cover the transaction
        var balance = await GetWalletStatus(user);
        if (balance.Balance < amount)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.InsufficientFunds,
                ErrorMessage = ErrorCodes.InsufficientFunds,
                PropertyName = null
            };
        }
        //create the transaction
        var newTransaction = new PaymentTransaction
        {
            //$"Payment for visit in: {visit.Restaurant.Name} on: {visit.Reservation.StartTime.ToShortDateString()}"
            Title = title,
            Amount = amount * -1,
            Time = DateTime.UtcNow,
            UserId = user.Id,
            User = user
        };
        await context.PaymentTransactions.AddAsync(newTransaction);
        await context.SaveChangesAsync();
        return new TransactionVM
        {
            TransactionId = newTransaction.PaymentTransactionId,
            Title = newTransaction.Title,
            Amount = newTransaction.Amount,
            Time = newTransaction.Time,
        };
    }
}
