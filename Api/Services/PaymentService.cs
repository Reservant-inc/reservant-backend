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
    [MethodErrorCodes<WalletService>(nameof(WalletService.DebitAsync))]
    [MethodErrorCodes<BankService>(nameof(BankService.SendMoneyToRestaurantAsync))]
    public async Task<Result<TransactionVM>> PayDepositAsync(User user, int visitId)
    {
        var visit = await context.Visits
            .Where(v => v.VisitId == visitId)
            .Include(v => v.Reservation)
            .Include(v => v.Restaurant)
            .FirstOrDefaultAsync();

        //check if the visit exists
        if (visit == null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.NotFound,
                ErrorMessage = ErrorCodes.NotFound,
                PropertyName = null,
            };
        }

        //check if the visit belongs to the user
        if (visit.ClientId != user.Id)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.AccessDenied,
                ErrorMessage = ErrorCodes.AccessDenied,
                PropertyName = null,
            };
        }

        //check if reservation requires a deposit
        if (visit.Reservation!.Deposit is null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.DepositFreeVisit,
                ErrorMessage = ErrorCodes.DepositFreeVisit,
                PropertyName = null,
            };
        }

        //check if the deposit was already paid
        if (visit.Reservation!.DepositPaymentTime is not null)
        {
            return new ValidationFailure
            {
                ErrorCode = ErrorCodes.DepositAlreadyMade,
                ErrorMessage = ErrorCodes.DepositAlreadyMade,
                PropertyName = null,
            };
        }

        var transaction = await walletService.DebitAsync(user,
            $"Payment for visit in: {visit.Restaurant.Name} on: {visit.Reservation.StartTime.ToShortDateString()}",
            visit.Reservation.Deposit.Value);
        if (transaction.IsError)
        {
            return transaction;
        }

        bankService.SendMoneyToRestaurantAsync(visit.Restaurant, (decimal)visit.Reservation.Deposit);

        visit.Reservation.DepositPaymentTime = transaction.Value.Time;
        await context.SaveChangesAsync();

        return transaction;
    }
}
