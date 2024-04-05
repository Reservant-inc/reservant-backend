using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Reservant.Api.Models;
using Reservant.Api.Options;
using Reservant.Api.Services;
using System.IdentityModel.Tokens.Jwt;

namespace Reservant.Api.Controllers
{
    [ApiController, Route("/users")]
    public class UsersController(UserService userService, UserManager<User> userManager, IOptions<JwtOptions> jwtOptions) : Controller
    {

        private readonly JwtSecurityTokenHandler _handler = new();
    }
}
