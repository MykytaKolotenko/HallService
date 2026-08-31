using System.Data;
using Hall_rent.Context;
using Hall_rent.Exceptions;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hall_rent.Repository;

public class HallUnitOfWork : IHallUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly ExceptionDispatcher _exceptionDispatcher;

    public HallUnitOfWork(AppDbContext dbContext, ExceptionDispatcher exceptionDispatcher)
    {
        _dbContext = dbContext;
        _exceptionDispatcher = exceptionDispatcher;
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default(CancellationToken))
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex)
            when (SqlErrorClassifier.IsUniqueViolation(ex))
        {
            throw new UniqueConstraintException(
                "Halls.Name",
                ex);
        }
    }

    public async Task<T> RunInTransactionAsync<T>(
        IsolationLevel isolationLevel,
        Func<Task<T>> operation,
        string operationName)
    {
        await using var transaction = await BeginTransactionAsync(isolationLevel);

        try
        {
            var result = await operation();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await SafeRollbackAsync(transaction);
            throw _exceptionDispatcher.Resolve(ex, operationName).Exception;
        }
    }

    private async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        return await _dbContext.Database.BeginTransactionAsync(isolationLevel);
    }

    private async Task SafeRollbackAsync(IDbContextTransaction transaction)
    {
        try
        {
            await transaction.RollbackAsync();
        }
        catch (Exception rollbackEx)
        {
            throw new InvalidOperationException("Rollback failed", rollbackEx);
        }
    }
}
