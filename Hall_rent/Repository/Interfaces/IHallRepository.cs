using Hall_rent.Entity;

namespace Hall_rent.Repository.Interfaces;

public interface IHallRepository
{
    Task AddAsync(HallEntity hall);
    Task<HallEntity?> GetByIdWithFavorsAsync(Guid id);
    void Remove(HallEntity hall);

    Task<List<HallEntity>> FindAvailableHallsAsync(DateTime from, DateTime to, int persons);
}