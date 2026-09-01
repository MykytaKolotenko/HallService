using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public sealed class DateRangeValidator : AbstractValidator<DateRangeRequest>
{
    public DateRangeValidator()
    {
        this.ApplyRangeRules();
    }
}