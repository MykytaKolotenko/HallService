using FluentValidation;
using Hall_rent.Request.Interface;

namespace Hall_rent.Validation.Rules;

public static class HallValidationRules
{
    public static void ApplyHallRules<T>(this AbstractValidator<T> validator) where T : IHallRequest
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(200).WithMessage("Hall name must not exceed 200 characters.");

        validator.RuleFor(x => x.Persons)
            .GreaterThan(0).WithMessage("Persons must be greater than 0.");

        validator.RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}
