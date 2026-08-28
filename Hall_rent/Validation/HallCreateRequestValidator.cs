using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class HallCreateRequestValidator : AbstractValidator<HallCreateRequest>
{
    public HallCreateRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Hall name is required.")
            .MaximumLength(200).WithMessage("Hall name must not exceed 200 characters.");

        RuleFor(x => x.Persons)
            .GreaterThan(0)
            .WithMessage("Capacity must be greater than 0.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");
    }
}