using FluentValidation;
using Reservant.Api.Models.Dtos.Employment;

namespace Reservant.Api.Validators.Employee
{
    public class UpdateEmploymentRequestValidator : AbstractValidator<UpdateEmploymentRequest>
    {
        public UpdateEmploymentRequestValidator()
        {
            RuleFor(e => new Tuple<bool, bool>(e.IsBackdoorEmployee, e.IsHallEmployee))
                    .AtLeastOneEmployeeRole();
        }
    }
}
