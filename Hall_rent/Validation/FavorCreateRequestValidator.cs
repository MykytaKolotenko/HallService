using FluentValidation;
using Hall_rent.Request;

namespace Hall_rent.Validation;

public class FavorCreateRequestValidator : AbstractValidator<FavorCreateRequest>
{
    public FavorCreateRequestValidator()
    {
        this.ApplyFavorRules();
    }
}