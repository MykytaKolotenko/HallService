using System.Net;
using FluentValidation;

namespace Hall_rent.Exceptions.Handling;

// Ловит FluentValidation.ValidationException независимо от того, где он выброшен:
// ValidatorUtils.Validate в контроллере, ручная валидация внутри сервиса и т.д.
// Именно поэтому такая логика должна жить в middleware/диспетчере, а не размазываться
// по try/catch в каждом контроллере или сервисе.
public class ValidationExceptionResolver : IExceptionResolver
{
    public bool CanHandle(Exception ex) => ex is ValidationException;

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var validationEx = (ValidationException)ex;

        var errors = validationEx.Errors
            .Select(e => $"{e.PropertyName}: {e.ErrorMessage}")
            .ToList();

        if (errors.Count == 0)
        {
            errors.Add(validationEx.Message);
        }

        return new ExceptionResolution(errors, HttpStatusCode.BadRequest, "ValidationError", LogLevel.Information, ex);
    }
}