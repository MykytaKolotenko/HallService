using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Exceptions.Handling;

// Единственный обработчик, который не наследник AppException —
// ловит сырую SqlException/DbUpdateException и оборачивает в ConcurrencyConflictException
public class SerializationConflictHandler : IExceptionHandler
{
    public bool CanHandle(Exception ex)
    {
        var sqlEx = ex as SqlException
                    ?? (ex as DbUpdateException)?.InnerException as SqlException;

        return sqlEx?.Number is 1205 or 3960;
    }

    public ExceptionResolution Resolve(Exception ex, string context)
    {
        var mapped = new ConcurrencyConflictException(context, ex);
        return new ExceptionResolution(mapped, mapped.StatusCode, mapped.Title, mapped.LogLevel);
    }
}