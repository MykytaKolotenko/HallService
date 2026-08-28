using Hall_rent.Entity;

namespace Hall_rent.Repository.Hall;

public interface IBookingRepository
{
    Task AddAsync(HallBookingEntity booking);
    Task<bool> IsHallAvailableAsync(Guid hallId, DateTime startAt, DateTime endAt);
}