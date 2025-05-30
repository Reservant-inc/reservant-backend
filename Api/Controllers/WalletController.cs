﻿using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Wallets;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

/// <summary>
/// Controller for handling customer transactions
/// </summary>
/// <param name="userManager"></param>
/// <param name="walletService"></param>
/// <param name="paymentService"></param>
[ApiController, Route("/wallet")]
[Authorize(Roles = Roles.Customer)]
public class WalletController(
    UserManager<User> userManager,
    WalletService walletService,
    PaymentService paymentService
): StrictController
{

    /// <summary>
    /// Creates a new transaction for the current user
    /// </summary>
    /// <param name="moneyRequest"></param>
    /// <returns></returns>
    [HttpPost("add-money")]
    [ProducesResponseType(204), ProducesResponseType(400)]
    [MethodErrorCodes<WalletService>(nameof(WalletService.CreateTransaction))]
    public async Task<ActionResult> CreateTransaction(AddMoneyRequest moneyRequest)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await walletService.CreateTransaction(moneyRequest, user);
        return OkOrErrors(result);
    }


    /// <summary>
    /// Gets the status of user's wallet
    /// </summary>
    /// <returns></returns>
    [HttpGet("status")]
    [ProducesResponseType(200)]
    public async Task<ActionResult<WalletStatusVM>> GetWalletStatus()
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        return await walletService.GetWalletStatus(user);
    }


    /// <summary>
    /// Gets all of user's transactions
    /// </summary>
    /// <param name="page"></param>
    /// <param name="perPage"></param>
    /// <returns></returns>
    [HttpGet("history")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<WalletService>(nameof(WalletService.GetTransactionHistory))]
    public async Task<ActionResult<Pagination<TransactionVM>>> GetTransactionHistory([FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await walletService.GetTransactionHistory(page, perPage, user);
        return OkOrErrors(result);

    }

    /// <summary>
    /// Makes a deposit payment for a specified visit
    /// </summary>
    /// <param name="visitId">id of the visit that requires the deposit</param>
    /// <returns>payment confirmation</returns>
    [HttpPost("pay-deposit")]
    [ProducesResponseType(200), ProducesResponseType(400)]
    [MethodErrorCodes<PaymentService>(nameof(PaymentService.PayDepositAsync))]
    public async Task<ActionResult<TransactionVM>> PayDeposit([FromQuery] int visitId) {
        var user = await userManager.GetUserAsync(User);
        if(user is null)
        {
            return Unauthorized();
        }
        var result = await paymentService.PayDepositAsync(user, visitId);
        return OkOrErrors(result);
    }
}
