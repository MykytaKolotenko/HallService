using Hall_rent.Entity;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Exceptions.Handling;

// Converts a "raw" DbUpdateException (a unique index violation in SQL Server) into a specific
// domain exception with a clear message. DbUpdateException.Entries contains the EF Core entities
// that participated in the failed SaveChanges operation — by their CLR type, we determine
// which uniqueness rule was violated.
public static class UniqueConstraintExceptionFactory
{
    public static AppException Create(Exception ex)
    {
        var entries = (ex as DbUpdateException)?.Entries ?? [];

        foreach (var entry in entries)
        {
            switch (entry.Entity)
            {
                case HallEntity hall:
                    return new HallNameAlreadyExistsException(hall.Name, ex);
            }
        }

        // Fallback for all other unique violations: it prevents the client from receiving a raw SQL error,
        // but it does not try to guess the exact cause — it simply names the entity type.
        var unknownEntityType = entries.FirstOrDefault()?.Entity.GetType().Name ?? "Unknown";
        return new UniqueConstraintException(unknownEntityType, ex);
    }
}