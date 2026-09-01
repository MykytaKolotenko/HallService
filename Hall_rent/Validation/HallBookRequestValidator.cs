using FluentValidation;
using Hall_rent.Helpers;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class HallBookRequestValidator : AbstractValidator<HallBookRequest>
{
    public HallBookRequestValidator(IClock clock)
    {
        this.ApplyDateRules(clock);

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Persons must be greater than 0.");

        RuleForEach(x => x.Favors)
            .NotEqual(Guid.Empty)
            .WithMessage("Each favor id must be a valid non-empty GUID.")
            .When(x => x.Favors is { Count: > 0 });
    }
}