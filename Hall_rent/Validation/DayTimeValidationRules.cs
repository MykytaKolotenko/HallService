using FluentValidation;
using Hall_rent.Helpers;
using Hall_rent.Request.Interface;

namespace Hall_rent.Validation;

public static class DayTimeValidationRules
{
    public static void ApplyDateRules<T>(this AbstractValidator<T> validator, IClock clock) where T : IDateRange
    {
        validator.RuleFor(x => x.From).NotEmpty();
        validator.RuleFor(x => x.To).NotEmpty();

        validator.RuleFor(x => x.To)
            .GreaterThan(x => x.From)
            .WithMessage("Start date must be earlier than end date.");

        validator.RuleFor(x => x.From)
            .GreaterThan(clock.UtcNow)
            .WithMessage("Start date must be in the future.");
    }
}
