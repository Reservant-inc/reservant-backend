using Microsoft.AspNetCore.Identity;
using Reservant.Api.Models;
using Reservant.Api.Services;

namespace Reservant.Api.Controllers;

public class WalletController(
    UserManager<User> userManager,
    WalletService walletService
): StrictController
{

}
