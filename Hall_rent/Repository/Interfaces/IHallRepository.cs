using Hall_rent.Entity;

namespace Hall_rent.Repository.Interfaces;

public interface IHallRepository
{
    Task AddAsync(HallEntity hall);
    Task<HallEntity?> GetByIdAsync(Guid id);
    void Remove(HallEntity hall);

    Task<List<HallEntity>> FindAvailableHallsAsync(DateTime startAt, DateTime endAt, int persons);
}