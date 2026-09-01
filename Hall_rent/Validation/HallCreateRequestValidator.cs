using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class HallCreateRequestValidator : AbstractValidator<HallCreateRequest>
{
    public HallCreateRequestValidator()
    {
        this.ApplyHallRules();
    }
}