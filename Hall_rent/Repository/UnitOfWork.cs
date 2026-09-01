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