using FluentValidation;
using Reservant.Api.Dtos.Employments;

namespace Reservant.Api.Validators.Employees
{
    /// <summary>
    /// Validator for UpdateEmploymentRequest
    /// </summary>
    public class UpdateEmploymentRequestValidator : AbstractValidator<UpdateEmploymentRequest>
    {
        /// <inheritdoc />
        public UpdateEmploymentRequestValidator()
        {
            RuleFor(e => new Tuple<bool, bool>(e.IsBackdoorEmployee, e.IsHallEmployee))
                    .AtLeastOneEmployeeRole();
        }
    }
}
