using System.Data;

namespace Hall_rent.Repository.Interfaces;

public interface IHallUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    public Task<T> RunInTransactionAsync<T>(
        IsolationLevel isolationLevel,
        Func<Task<T>> operation,
        string operationName);
}