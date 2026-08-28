// Exceptions/SqlErrorClassifier.cs

using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Exceptions;

public static class SqlErrorClassifier
{
    public static bool IsSerializationFailure(Exception ex)
    {
        var sqlEx = ex as SqlException
                    ?? (ex as DbUpdateException)?.InnerException as SqlException;

        return sqlEx?.Number is 1205 or 3960;
    }
}