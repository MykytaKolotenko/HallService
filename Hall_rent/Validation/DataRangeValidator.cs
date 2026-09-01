using FluentValidation;
using Hall_rent.Helpers;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class DataRangeValidator : AbstractValidator<DateRangeRequest>
{
    public DataRangeValidator(IClock clock)
    {
        this.ApplyDateRules(clock);
    }
}