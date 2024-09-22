using FluentValidation;

namespace ErrorCodeDocs.Test.SampleData;

public class SampleValidator : AbstractValidator<SampleDto>
{
    public SampleValidator()
    {
        RuleFor(x => x.Property1)
            .Must(x => true)
            .WithErrorCode(ErrorCodes.ValidatorFirstCode)
            .WithMessage("Description from validator");

        RuleFor(x => x.Property2)
            .Must(x => true)
            .WithErrorCode(ErrorCodes.ValidatorSecondCode)
            .WithMessage("Description from validator");
    }
}
