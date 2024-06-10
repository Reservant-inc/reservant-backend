using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

/// <summary>
/// controller for handling user transactions
/// </summary>

[ApiController, Route("/wallet")]
public class WalletController(
    UserManager<User> userManager,
    WalletService walletService
    ) : StrictController
{

}
