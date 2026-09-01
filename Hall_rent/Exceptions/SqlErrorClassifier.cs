using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Exceptions;

/// Classifies SQL Server errors by number (SqlException.Number). EF Core wraps SqlException
// into DbUpdateException during SaveChanges, so in both methods we first try to extract
// SqlException directly, and if that fails, from the InnerException of DbUpdateException.
public static class SqlErrorClassifier
{
    /// <summary>
    /// 1205 is the classic deadlock ("Transaction was deadlocked on lock resources").
    /// 3960 is a snapshot conflict under Serializable/Snapshot isolation ("Snapshot isolation transaction
    /// aborted due to update conflict") — this is the code you get when two parallel Serializable booking
    /// transactions (see BookingService.BookAsync) try to modify overlapping data at the same time.
    /// Both cases are not data errors, but a race condition that should be retried on the client side
    /// (see SerializationConflictResolver -> 409 Conflict).
    /// </summary>
    public static bool IsSerializationFailure(Exception ex)
    {
        var sqlEx = ex as SqlException
                    ?? (ex as DbUpdateException)?.InnerException as SqlException;

        return sqlEx?.Number is 1205 or 3960;
    }

    /// <summary>
    /// 2601 is a unique index violation (duplicate key row).
    /// 2627 is a PRIMARY KEY/UNIQUE constraint violation.
    /// In this project, this applies mainly to the unique hall name (HallEntity.Name)
    /// and the unique (HallBookingId, FavorId) pair — see AppDbContext.OnModelCreating.
    /// </summary>
    public static bool IsUniqueViolation(Exception ex)
    {
        var sqlEx = ex as SqlException
                    ?? (ex as DbUpdateException)?.InnerException as SqlException;

        return sqlEx?.Number is 2601 or 2627;
    }
}