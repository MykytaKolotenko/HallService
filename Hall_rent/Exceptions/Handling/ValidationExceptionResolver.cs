using System.Net;
using FluentValidation;

namespace Hall_rent.Exceptions.Handling;

// Catches FluentValidation.ValidationException no matter where it is thrown:
// ValidatorUtils.Validate in the controller, manual validation inside a service, etc.
public class ValidationExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex)
    {
        return ex is ValidationException;
    }

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var validationEx = (ValidationException)ex;

        var errors = validationEx.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        if (errors.Count == 0)
            errors.Add(validationEx.Message);

        return new ExceptionResolution(errors, HttpStatusCode.BadRequest, "ValidationError", LogLevel.Information, ex);
    }
}
