using FluentValidation;
using Hall_rent.Request;

public class HallSearchRequestValidator : AbstractValidator<HallSearchRequest>
{
    public HallSearchRequestValidator()
    {
        RuleFor(x => x.StartAt)
            .LessThan(x => x.EndAt)
            .WithMessage("StartAt must be earlier than EndAt.");

        RuleFor(x => x.EndAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("End date must be in the future.");

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Persons must be greater than 0.");
    }
}