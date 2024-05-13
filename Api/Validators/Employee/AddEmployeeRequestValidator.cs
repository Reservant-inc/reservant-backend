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

        public AddEmployeeRequestValidator() {
            RuleFor(e => e.Id)
                .NotNull();

            RuleFor(e => new Tuple<bool,bool>(e.IsHallEmployee, e.IsBackdoorEmployee))
                .AtLeastOneEmployeeRole();

        }
    }
}
