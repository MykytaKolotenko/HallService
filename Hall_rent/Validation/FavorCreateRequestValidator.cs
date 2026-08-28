using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class FavorCreateRequestValidator : AbstractValidator<FavorCreateRequest>
{
    public FavorCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Favor name is required.")
            .MaximumLength(150).WithMessage("Favor name must not exceed 150 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");
    }
}