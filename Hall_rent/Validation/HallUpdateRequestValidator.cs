using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class HallUpdateRequestValidator : AbstractValidator<HallUpdateRequest>
{
    public HallUpdateRequestValidator()
    {
        this.ApplyHallRules();

        RuleFor(x => x.Favors)
            .NotNull()
            .WithMessage("Favors collection cannot be null.");

        RuleForEach(x => x.Favors!)
            .NotEqual(Guid.Empty)
            .WithMessage("Each favor id must be a valid non-empty GUID.")
            .When(x => x.Favors != null && x.Favors.Count > 0);
    }
}