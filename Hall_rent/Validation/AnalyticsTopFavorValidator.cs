using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class AnalyticsTopFavorValidator : AbstractValidator<AnalyticsTopFavorRequest>
{
    public AnalyticsTopFavorValidator()
    {
        this.ApplyRangeRules();

        RuleFor(x => x.Limit)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);
    }
}