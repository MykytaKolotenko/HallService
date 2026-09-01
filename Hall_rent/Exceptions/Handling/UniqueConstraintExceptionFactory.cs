using Hall_rent.Entity;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Exceptions.Handling;

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

        var unknownEntityType = entries.FirstOrDefault()?.Entity.GetType().Name ?? "Unknown";
        return new UniqueConstraintException(unknownEntityType, ex);
    }
}