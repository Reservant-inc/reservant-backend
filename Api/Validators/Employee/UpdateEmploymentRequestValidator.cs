using FluentValidation;
using Reservant.Api.Models.Dtos.Employment;

namespace Reservant.Api.Validators.Employee
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
