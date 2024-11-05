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
/// <param name="transactionService"></param>
public class WalletService(
    ApiDbContext context,
    ValidationService validationService,
    TransactionService transactionService)
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
    /// <param name="visitId">id of the visit that the deposit is for</param>
    /// <returns>DTO with confirmation of the payment</returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    [ErrorCode(null, ErrorCodes.DepositFreeVisit)]
    [ErrorCode(null, ErrorCodes.DepositAlreadyMade)]
    [ErrorCode(null, ErrorCodes.InsufficientFunds)]
    [MethodErrorCodes<WalletService>(nameof(GetWalletStatus))]
    public async Task<Result<TransactionVM>> PayDepositAsync(User user, int visitId)
    {
        var visit = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Include(v => v.Reservation)
            .FirstOrDefaultAsync();
        //check if the visit exists
        if (visit == null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
                PropertyName = null
            };
        }
        //check if the visit belongs to the user
        if (visit.ClientId != user.Id)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = ErrorCodes.AccessDenied,
                PropertyName = null
            };
        }
        //check if reservation requires a deposit
        if (visit.Reservation!.Deposit is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.DepositFreeVisit,
                ErrorMessage = ErrorCodes.DepositFreeVisit,
                PropertyName = null
            };
        }
        //check if the deposit was already paid
        if (visit.Reservation!.DepositPaymentTime is not null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.DepositAlreadyMade,
                ErrorMessage = ErrorCodes.DepositAlreadyMade,
                PropertyName = null
            };
        }
        var newTransaction = await transactionService.MakeTransactionAsync(
            user,
            $"Deposit payment for visit in restaurant: {visit.Restaurant.Name} on: {visit.Reservation.StartTime}",
            (decimal)visit.Reservation.Deposit * -1);
        if (newTransaction == null) {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.InsufficientFunds,
                ErrorMessage = ErrorCodes.InsufficientFunds,
                PropertyName = null
            };
        }
        visit.Reservation.DepositPaymentTime = newTransaction!.Time;
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
