using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Dtos.Restaurants;

namespace Reservant.Api.Validators.Employees
{
    /// <summary>
    /// Validator for AddEmployeeRequest
    /// </summary>
    public class AddEmployeeRequestValidator : AbstractValidator<AddEmployeeRequest>
    {
        /// <inheritdoc />
        public AddEmployeeRequestValidator(ApiDbContext context) {
            RuleFor(e => e.EmployeeId)
                .CurrentUsersEmployee(context);

            RuleFor(e => new Tuple<bool,bool>(e.IsHallEmployee, e.IsBackdoorEmployee))
                .AtLeastOneEmployeeRole();

        }
    }
}
