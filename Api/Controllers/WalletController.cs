using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos;
using Reservant.Api.Models.Dtos.Order;
using Reservant.Api.Models.Dtos.Wallet;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

[ApiController, Route("/wallet")]
[Authorize(Roles = Roles.Customer)]
public class WalletController(
    UserManager<User> userManager,
    WalletService walletService
): StrictController
{
    [HttpPost("/add-money")]
    public async Task<ActionResult> CreateTransaction(AddMoneyRequest moneyRequest)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await walletService.CreateTransaction(moneyRequest, user!);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok();
    }

    [HttpGet("/status")]
    public async Task<ActionResult<WalletStatusVM>> GetWalletStatus()
    {
        var user = await userManager.GetUserAsync(User);
        var result = await walletService.GetWalletStatus(user!);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);
    }

    [HttpGet("/history")]
    public async Task<ActionResult<Pagination<TransactionVM>>> GetTransactionHistory([FromQuery] int page = 0, [FromQuery] int perPage = 10)
    {
        var user = await userManager.GetUserAsync(User);
        var result = await walletService.GetTransactionHistory(page, perPage, user!);

        if (result.IsError)
        {
            return result.ToValidationProblem();
        }

        return Ok(result.Value);

    }

}
