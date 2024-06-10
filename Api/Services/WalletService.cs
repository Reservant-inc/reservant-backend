﻿using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reservant.Api.Data;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Wallet;
using Reservant.Api.Validation;
using Reservant.Api.Validators;

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
    public async Task<Result<PaymentTransaction>> CreateTransaction(AddMoneyRequest moneyRequest, User user)
    {
        var newTransaction = new PaymentTransaction
        {
            Title = moneyRequest.Title,
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

    /// <summary>
    /// gets given user's wallet status
    /// </summary>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<WalletStatusVM>> GetWalletStatus(User user)
    {
        var transactions = await context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .ToListAsync();

        if (transactions == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(user.Id),
                ErrorMessage = $"No transactions found for user {user.Id}",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return new WalletStatusVM
        {
            Balance = transactions.Sum(p => p.Amount)
        };
    }

    /// <summary>
    /// gets the transaction history for the requested user
    /// </summary>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <param name="user"></param>
    /// <returns></returns>
    public async Task<Result<Pagination<TransactionVM>>> GetTransactionHistory(int page, int perPage, User user)
    {
        var transactions = context.PaymentTransactions
            .Where(p => p.UserId == user.Id)
            .Select(p => new TransactionVM
            {
                TransactionId = p.Id,
                Title = p.Title,
                Amount = p.Amount,
                Time = p.Time,
            });

        if (transactions == null)
        {
            return new ValidationFailure
            {
                PropertyName = nameof(user.Id),
                ErrorMessage = $"No transactions found for user {user.Id}",
                ErrorCode = ErrorCodes.NotFound
            };
        }

        return await transactions.PaginateAsync(page, perPage);

    }

}
