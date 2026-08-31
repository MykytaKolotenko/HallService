using FluentValidation;
using Hall_rent.Request.Interface;

namespace Hall_rent.Validation;

public static class FavorValidationRules
{
    public static void ApplyFavorRules<T>(this AbstractValidator<T> validator) where T : IFavorRequest
    {
        validator.RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Favor name is required.")
            .MaximumLength(150).WithMessage("Favor name must not exceed 150 characters.");

        validator.RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}