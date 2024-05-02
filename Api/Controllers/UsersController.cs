using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Reservant.Api.Identity;
using Reservant.Api.Models;
using Reservant.Api.Models.Dtos.User;
using Reservant.Api.Services;
using Reservant.Api.Validation;

namespace Reservant.Api.Controllers
{
    [ApiController, Route("/users")]
    public class UsersController(UserService userService) : Controller
    {
        /// <summary>
        /// Sets Restaurant Owner role for specified user
        /// </summary>
        /// <param name="id">id of the user</param>
        /// <returns></returns>
        [ProducesResponseType(200), ProducesResponseType(404)]
        [HttpPost("{id}/make-restaurant-owner"), Authorize(Roles = Roles.CustomerSupportAgent)]
        public async Task<ActionResult> MakeRestaurantOwner(string id) {
            var result = await userService.MakeRestaurantOwnerAsync(id);
            if (result.IsError)
            {
                ValidationUtils.AddErrorsToModel(result.Errors!, ModelState);
                return ValidationProblem();
            }
            User user = result.Value;
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
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }
           
            var emp = result.Value;

            return Ok(new UserDetailsVM
            {
                Id = emp.Id,
                Login = emp.UserName!,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                RegisteredAt = emp.RegisteredAt,
                BirthDate = emp.BirthDate,
                Roles = await userService.GetRolesAsync(emp),
                EmployerId = emp.EmployerId,
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
                ValidationUtils.AddErrorsToModel(result.Errors, ModelState);
                return ValidationProblem();
            }

            var emp = result.Value;

            return Ok(new UserDetailsVM
            {
                Id = emp.Id,
                Login = emp.UserName!,
                Email = emp.Email,
                PhoneNumber = emp.PhoneNumber,
                FirstName = emp.FirstName,
                LastName = emp.LastName,
                RegisteredAt = emp.RegisteredAt,
                BirthDate = emp.BirthDate,
                Roles = await userService.GetRolesAsync(emp),
                EmployerId = emp.EmployerId,
            });
        }
    }
}
