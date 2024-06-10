using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.Wallet;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers;

public class WalletController(
    UserManager<User> userManager,
    WalletService walletService
): StrictController
{
    [HttpPost]
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

}
