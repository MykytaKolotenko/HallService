using FluentValidation;

namespace Hall_rent.Validation;

public static class ValidatorUtils
{
    public static async Task Validate<T>(IValidator<T> validator, T data)
    {
        var result = await validator.ValidateAsync(data);

        if (result.IsValid) return;

        throw new ValidationException(result.Errors);
    }
}