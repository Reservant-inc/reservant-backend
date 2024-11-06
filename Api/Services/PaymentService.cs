using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Wallets;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using System.ComponentModel.DataAnnotations;

namespace Reservant.Api.Services;

/// <summary>
/// Srevice for managing payments
/// </summary>
/// <param name="context">database context</param>
/// <param name="bankService"></param>
/// <param name="walletService"></param>
public class PaymentService(
    ApiDbContext context,
    BankService bankService,
    WalletService walletService)
{
    /// <summary>
    /// Function for paying the reservation deposit
    /// </summary>
    /// <param name="user"></param>
    /// <param name="visitId"></param>
    /// <returns></returns>
    [ErrorCode(null, ErrorCodes.NotFound)]
    [ErrorCode(null, ErrorCodes.AccessDenied)]
    [ErrorCode(null, ErrorCodes.DepositFreeVisit)]
    [ErrorCode(null, ErrorCodes.DepositAlreadyMade)]
    [ErrorCode(null, ErrorCodes.InsufficientFunds)]
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
        //check for sufficient funds to cover the transaction
        var balance = await context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .SumAsync(p => p.Amount);
        if (visit.Reservation.Deposit < 0 && balance < visit.Reservation.Deposit * -1)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.InsufficientFunds,
                ErrorMessage = ErrorCodes.InsufficientFunds,
                PropertyName = null
            };
        }
        bankService.SendMoneyToRestaurantAsync(visit.Restaurant, visit.Reservation.Deposit);
        var transaction = await walletService.PayDepositAsync(user, visit);
        if (transaction.IsError) {
            return transaction;
        }
        visit.Reservation.DepositPaymentTime = transaction.Value.Time;
        await context.SaveChangesAsync();
        return transaction;
    }
}
