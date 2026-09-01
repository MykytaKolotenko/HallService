using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class AnalyticsTopFavorValidator : AbstractValidator<AnalyticsTopFavorRequest>
{
    public AnalyticsTopFavorValidator()
    {
        this.ApplyRangeRules();

        this.LimitValidator(x => x.Limit);
    }
}