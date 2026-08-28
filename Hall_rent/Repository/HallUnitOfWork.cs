using System.Data;
using Hall_rent.Context;
using Hall_rent.Exceptions.Handling;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hall_rent.Repository;

public class HallUnitOfWork : IHallUnitOfWork
{
    private readonly AppDbContext _dbContext;
    private readonly ExceptionDispatcher _exceptionDispatcher;
    private readonly ILogger<HallUnitOfWork> _logger;

    public HallUnitOfWork(AppDbContext dbContext, ILogger<HallUnitOfWork> logger, ExceptionDispatcher exceptionDispatcher)
    {
        _dbContext = dbContext;
        _logger = logger;
        _exceptionDispatcher = exceptionDispatcher;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.SaveChangesAsync(cancellationToken);
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
            _logger.LogError(rollbackEx, "Rollback failed");
        }
    }
}
