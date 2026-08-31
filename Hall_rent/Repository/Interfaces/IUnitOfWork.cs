namespace Hall_rent.Repository.Interfaces;

public interface IUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default(CancellationToken));
}