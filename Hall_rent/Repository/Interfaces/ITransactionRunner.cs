using System.Data;

namespace Hall_rent.Repository.Interfaces;

public interface ITransactionRunner
{
    public Task<T> RunInTransactionAsync<T>(IsolationLevel isolationLevel, Func<Task<T>> operation);
}