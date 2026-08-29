using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class HallBookRequestValidator : AbstractValidator<HallBookRequest>
{
    public HallBookRequestValidator()
    {
        RuleFor(x => x.StartAt)
            .GreaterThan(DateTime.UtcNow)
            .WithMessage("StartAt must be in the future.");

        RuleFor(x => x.EndAt)
            .NotEmpty()
            .GreaterThan(x => x.StartAt)
            .WithMessage("EndAt must be greater than StartAt.");

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Persons must be greater than 0.");

        RuleForEach(x => x.Favors)
            .NotEqual(Guid.Empty)
            .WithMessage("Each favor id must be a valid non-empty GUID.")
            .When(x => x.Favors is { Count: > 0 });
    }
}