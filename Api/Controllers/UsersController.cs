using Reservant.ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.Users;
using Reservant.Api.Identity;
using Reservant.Api.Mapping;
using Reservant.Api.Models;
using Reservant.Api.Services;
using Reservant.Api.Validation;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Fetching information about other users and managing employees
    /// </summary>
    [ApiController, Route("/users")]
    public class UsersController(
        UserService userService, 
        UrlService urlService,
        UserManager<User> userManager) : StrictController
    {
        /// <summary>
        /// Find users
        /// </summary>
        /// <remarks>
        /// Only searches for customers. Returns friends first,
        /// then friend requests, then strangers.
        /// </remarks>
        /// <param name="name">Search by user's name</param>
        /// <param name="filter">Filter results</param>
        /// <param name="page">Page number</param>
        /// <param name="perPage">Items per page</param>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [MethodErrorCodes<UserService>(nameof(UserService.FindUsersAsync))]
        public async Task<ActionResult<Pagination<FoundUserVM>>> FindUsers(
            string name, UserSearchFilter filter = UserSearchFilter.NoFilter,
            int page = 0, int perPage = 10)
        {
            var result = await userService.FindUsersAsync(
                name, filter, User.GetUserId()!.Value, page, perPage);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Sets Restaurant Owner role for specified user
        /// </summary>
        /// <param name="userId">id of the user</param>
        /// <returns></returns>
        [ProducesResponseType(200), ProducesResponseType(404)]
        [HttpPost("{userId}/make-restaurant-owner"), Authorize(Roles = Roles.CustomerSupportAgent)]
        public async Task<ActionResult> MakeRestaurantOwner(Guid userId) {
            var user = await userService.MakeRestaurantOwnerAsync(userId);
            if (user == null){ return NotFound(); }
            return Ok();
        }

        /// <summary>
        /// Updates employee who works for the current user
        /// </summary>
        /// <param name="request"></param>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpPut("{employeeId}")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<ActionResult<UserDetailsVM>> PutEmployee(UpdateUserDetailsRequest request, Guid employeeId)
        {
            var result = await userService.PutEmployeeAsync(request, employeeId, User);

            if (result.IsError)
            {
                return result.ToValidationProblem();
            }

            var emp = result.Value;

            return Ok(new UserDetailsVM
            {
                UserId = emp.Id,
                Login = emp.UserName!,
                Email = emp.Email!,
                PhoneNumber = emp.FullPhoneNumber,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                RegisteredAt = emp.RegisteredAt,
                BirthDate = emp.BirthDate,
                Roles = await userService.GetRolesAsync(emp),
                EmployerId = emp.EmployerId,
                Photo = urlService.GetPathForFileName(emp.PhotoFileName),
            });
        }

        /// <summary>
        /// Gets information about a specific user.
        /// </summary>
        /// <param name="userId">ID of the user to retrieve</param>
        /// <returns></returns>
        [HttpGet("{userId}")]
        [Authorize]
        [ProducesResponseType(200), ProducesResponseType(401), ProducesResponseType(403), ProducesResponseType(400)]
        [MethodErrorCodes<UserService>(nameof(UserService.GetUserDetailsAsync))]
        public async Task<ActionResult<UserEmployeeVM>> GetUserById(Guid userId)
        {
            var result = await userService.GetUserDetailsAsync(userId, User.GetUserId()!.Value);
            return OkOrErrors(result);
        }
        
        /// <summary>
        /// Ban user based on id 
        /// </summary>
        [HttpPost("/{userId}/ban")]
        [Authorize(Roles = $"{Roles.CustomerSupportAgent}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> BanUser(
            Guid userId,
            BanDto dto
        )
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await userService.BanUserAsync(user,userId,dto);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Unban user based on id 
        /// </summary>
        [HttpPost("/{userId}/unban")]
        [Authorize(Roles = $"{Roles.CustomerSupportAgent}")]
        [ProducesResponseType(200), ProducesResponseType(400)]
        public async Task<ActionResult> UnbanUser(
            Guid userId
        )
        {
            var user = await userManager.GetUserAsync(User);
            if (user is null)
            {
                return Unauthorized();
            }

            var result = await userService.UnbanUserAsync(user,userId);
            return OkOrErrors(result);
        }

        
    }
}
