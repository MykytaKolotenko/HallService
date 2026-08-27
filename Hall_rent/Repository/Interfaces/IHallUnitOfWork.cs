namespace Hall_rent.Repository.Hall;

public interface IHallUnitOfWork
{
    Task<int> SaveChangesAsync();
}
