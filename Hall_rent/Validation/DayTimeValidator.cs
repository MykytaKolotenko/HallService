using FluentValidation;
using Hall_rent.Helpers;
using Hall_rent.Request;

public class DateRangeRequestValidator : AbstractValidator<DateRangeRequest>
{
    public DateRangeRequestValidator(IClock clock)
    {
        RuleFor(x => x.From).NotEmpty();
        RuleFor(x => x.To).NotEmpty();

        RuleFor(x => x.From)
            .LessThan(x => x.To)
            .WithMessage("StartAt must be earlier than EndAt.");

        RuleFor(x => x.From)
            .GreaterThan(clock.UtcNow)
            .WithMessage("End date must be in the future.");
    }
}