using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class FavorUpdateRequestValidator : AbstractValidator<FavorUpdateRequest>
{
    public FavorUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        this.ApplyFavorRules();
    }
}