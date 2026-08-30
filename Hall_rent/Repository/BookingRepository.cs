using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly DbSet<HallBookingEntity> _dbSet = context.Set<HallBookingEntity>();

    public async Task AddAsync(HallBookingEntity booking)
    {
        await _dbSet.AddAsync(booking);
    }

    public async Task<bool> IsHallAvailableAsync(Guid hallId, DateTime startAt, DateTime endAt)
    {
        return !await _dbSet.AnyAsync(b => b.HallId == hallId && b.StartAt < endAt && b.EndAt > startAt);
    }
}