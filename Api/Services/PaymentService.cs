using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Wallets;
using Reservant.Api.Models;
using Reservant.Api.Validation;
using Reservant.Api.Validators;
using Reservant.ErrorCodeDocs.Attributes;
using System.ComponentModel.DataAnnotations;
using Reservant.Api.Models.Enums;

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
    [ErrorCode(null, ErrorCodes.NotFound, "Visit not found")]
    [ErrorCode(null, ErrorCodes.AccessDenied, "Only the client who made the reservation can pay the deposit")]
    [ErrorCode(null, ErrorCodes.NoDepositToBePaid, "The reservation does not require a deposit or it has already been paid")]
    [MethodErrorCodes<WalletService>(nameof(WalletService.DebitAsync))]
    [MethodErrorCodes<BankService>(nameof(BankService.SendMoneyToRestaurantAsync))]
    public async Task<Result<TransactionVM>> PayDepositAsync(User user, int visitId)
    {
        var visit = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Include(v => v.Reservation)
            .Include(v => v.Restaurant)
            .FirstOrDefaultAsync();

        if (visit is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = "Visit not found",
                PropertyName = null,
            };
        }

        if (visit.ClientId != user.Id)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = "Only the client who made the reservation can pay the deposit",
                PropertyName = null,
            };
        }

        if (visit.Reservation?.CurrentStatus is not ReservationStatus.DepositNotPaid)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NoDepositToBePaid,
                ErrorMessage = "The reservation does not require a deposit or it has already been paid",
                PropertyName = null,
            };
        }

        var transaction = await walletService.DebitAsync(user,
            $"Payment for visit in: {visit.Restaurant.Name} on: {visit.Reservation.StartTime.ToShortDateString()}",
            visit.Reservation.Deposit!.Value);
        if (transaction.IsError)
        {
            return transaction;
        }

        bankService.SendMoneyToRestaurantAsync(visit.Restaurant, visit.Reservation.Deposit.Value);

        visit.Reservation.DepositPaymentTime = transaction.Value.Time;
        await context.SaveChangesAsync();

        return transaction;
    }
}
