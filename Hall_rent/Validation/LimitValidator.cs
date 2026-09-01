using FluentValidation;

namespace Hall_rent.Validation;

public sealed class LimitValidator : AbstractValidator<int>
{
    public LimitValidator()
    {
        RuleFor(x => x)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}