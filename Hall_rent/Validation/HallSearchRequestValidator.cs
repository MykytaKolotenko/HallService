using FluentValidation;
using Hall_rent.Helpers;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class HallSearchRequestValidator : AbstractValidator<HallSearchRequest>
{
    public HallSearchRequestValidator(IClock clock)
    {
        this.ApplyDateRules(clock);

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Persons must be greater than 0.");
    }
}
