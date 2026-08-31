using System.Data;
using Hall_rent.Context;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Hall_rent.Repository;

public class TransactionRunner : ITransactionRunner
{
    private readonly AppDbContext _dbContext;

    public TransactionRunner(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<T> RunInTransactionAsync<T>(
        IsolationLevel isolationLevel,
        Func<Task<T>> operation)
    {
        await using var transaction = await BeginTransactionAsync(isolationLevel);

        var result = await operation();
        await transaction.CommitAsync();
        return result;
    }

    private async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel)
    {
        return await _dbContext.Database.BeginTransactionAsync(isolationLevel);
    }
}