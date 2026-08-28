using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class HallUpdateRequestValidator : AbstractValidator<HallUpdateRequest>
{
    public HallUpdateRequestValidator()
    {
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Persons must be greater than 0.");

        RuleFor(x => x.Favors)
            .NotNull()
            .WithMessage("Favors collection cannot be null.");

        RuleForEach(x => x.Favors!)
            .NotEqual(Guid.Empty)
            .WithMessage("Each favor id must be a valid non-empty GUID.")
            .When(x => x.Favors != null && x.Favors.Count > 0);
    }
}