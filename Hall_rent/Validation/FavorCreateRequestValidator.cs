using FluentValidation;
using Hall_rent.Request;
using Hall_rent.Validation.Rules;

namespace Hall_rent.Validation;

public class FavorCreateRequestValidator : AbstractValidator<FavorCreateRequest>
{
    public FavorCreateRequestValidator()
    {
        this.ApplyFavorRules();
    }
}