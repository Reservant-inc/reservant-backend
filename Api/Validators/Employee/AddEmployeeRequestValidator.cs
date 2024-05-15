using FluentValidation;
using Reservant.Api.Data;
using Reservant.Api.Models.Dtos.Restaurant;

namespace Reservant.Api.Validators.Employee
{
    /// <summary>
    /// Validator for AddEmployeeRequest
    /// </summary>
    public class AddEmployeeRequestValidator : AbstractValidator<AddEmployeeRequest>
    {

        public AddEmployeeRequestValidator(ApiDbContext context) {
            RuleFor(e => e.Id)
                .NotNull()
                .CurrentUsersEmployee(context);

            RuleFor(e => new Tuple<bool,bool>(e.IsHallEmployee, e.IsBackdoorEmployee))
                .AtLeastOneEmployeeRole();

        }
    }
}
