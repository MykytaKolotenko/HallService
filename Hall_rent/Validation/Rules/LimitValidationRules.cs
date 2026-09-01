using System.Linq.Expressions;
using FluentValidation;

namespace Hall_rent.Validation.Rules;

public static class LimitValidationRules
{
    public static void LimitValidator<T>(this AbstractValidator<T> validator, Expression<Func<T, int>> selector)
    {
        validator.RuleFor(selector).SetValidator(new LimitValidator());
    }
}
