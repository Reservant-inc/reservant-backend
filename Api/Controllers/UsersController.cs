using ErrorCodeDocs.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Dtos;
using Reservant.Api.Dtos.User;
using Reservant.Api.Identity;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers
{
    /// <summary>
    /// Fetching information about other users and managing employees
    /// </summary>
    [ApiController, Route("/users")]
    public class UsersController(UserService userService, FileUploadService uploadService) : StrictController
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
                name, filter, User.GetUserId()!, page, perPage);
            return OkOrErrors(result);
        }

        /// <summary>
        /// Sets Restaurant Owner role for specified user
        /// </summary>
        /// <param name="userId">id of the user</param>
        /// <returns></returns>
        [ProducesResponseType(200), ProducesResponseType(404)]
        [HttpPost("{userId}/make-restaurant-owner"), Authorize(Roles = Roles.CustomerSupportAgent)]
        public async Task<ActionResult> MakeRestaurantOwner(string userId) {
            var user = await userService.MakeRestaurantOwnerAsync(userId);
            if (user == null){ return NotFound(); }
            return Ok();
        }

        /// <summary>
        /// Gets employee who works for the current user
        /// </summary>
        /// <param name="employeeId"></param>
        /// <returns></returns>
        [HttpGet("{employeeId}")]
        [Authorize(Roles = Roles.RestaurantOwner)]
        [ProducesResponseType(200), ProducesResponseType(400), ProducesResponseType(401)]
        public async Task<ActionResult<UserDetailsVM>> GetEmployee(string employeeId)
        {
            var result = await userService.GetEmployeeAsync(employeeId, User);

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
                PhoneNumber = emp.PhoneNumber!,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                RegisteredAt = emp.RegisteredAt,
                BirthDate = emp.BirthDate,
                Roles = await userService.GetRolesAsync(emp),
                EmployerId = emp.EmployerId,
                Photo = uploadService.GetPathForFileName(emp.PhotoFileName),
            });
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
        public async Task<ActionResult<UserDetailsVM>> PutEmployee(UpdateUserDetailsRequest request, string employeeId)
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
                PhoneNumber = emp.PhoneNumber!,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                RegisteredAt = emp.RegisteredAt,
                BirthDate = emp.BirthDate,
                Roles = await userService.GetRolesAsync(emp),
                EmployerId = emp.EmployerId,
                Photo = uploadService.GetPathForFileName(emp.PhotoFileName),
            });
        }
    }
}
