using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class FavorUpdateRequestValidator : AbstractValidator<FavorUpdateRequest>
{
    public FavorUpdateRequestValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        this.ApplyFavorRules();
    }
}