using FluentValidation;
using Reservant.Api.Models.Dtos.Restaurant;

namespace Reservant.Api.Validators.Employee
{
    /// <summary>
    /// Validator for AddEmployeeRequest
    /// </summary>
    public class AddEmployeeRequestValidator : AbstractValidator<AddEmployeeRequest>
    {
        public AddEmployeeRequestValidator() {
            RuleFor(e => e.IsHallEmployee || e.IsBackdoorEmployee)
                .NotEmpty()
                .NotNull()
                .Equals(true);
        }
    }
}
