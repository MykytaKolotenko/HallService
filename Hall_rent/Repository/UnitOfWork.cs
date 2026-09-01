using Hall_rent.Context;
using Hall_rent.Exceptions;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _dbContext;

    public UnitOfWork(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // We only handle unique constraint violations here (not, for example, serialization failures —
    // those are handled at a higher level in ExceptionDispatcher via SerializationConflictResolver),
    // because only for unique violations do we have access to EntityEntry (ex.Entries) and can build
    // a clear domain exception such as HallNameAlreadyExistsException with the name of the conflicting
    // entity — see UniqueConstraintExceptionFactory.Create.
    public async Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (SqlErrorClassifier.IsUniqueViolation(ex))
        {
            throw UniqueConstraintExceptionFactory.Create(ex);
        }
    }
}
