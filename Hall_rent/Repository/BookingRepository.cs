using Hall_rent.Entity;
using Hall_rent.Repository.Hall;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class BookingRepository(AppDbContext context) : IBookingRepository
{
    private readonly DbSet<HallBookingEntity> _dbSet = context.Set<HallBookingEntity>();

    public async Task AddAsync(HallBookingEntity booking)
    {
        await _dbSet.AddAsync(booking);
    }
}
