using FluentValidation;
using Hall_rent.Request;

public class FavorUpdateRequestValidator : AbstractValidator<FavorUpdateRequest>
{
    public FavorUpdateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Favor name is required.")
            .MaximumLength(150).WithMessage("Favor name must not exceed 150 characters.");

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Price must be greater than or equal to 0.");
    }
}