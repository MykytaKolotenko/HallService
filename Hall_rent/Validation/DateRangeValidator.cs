using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public sealed class DateRangeValidator : AbstractValidator<DateRangeRequest>
{
    public DateRangeValidator()
    {
        this.ApplyRangeRules();
    }
}