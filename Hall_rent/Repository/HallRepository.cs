using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class HallRepository : IHallRepository
{
    private readonly AppDbContext _context;
    private readonly DbSet<HallEntity> _dbSet;

    public HallRepository(AppDbContext context)
    {
        _context = context;
        _dbSet = context.Set<HallEntity>();
    }

    public async Task<HallEntity?> GetByIdWithFavorsAsync(Guid id)
    {
        return await _dbSet
            .Include(h => h.Favors)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(HallEntity hall)
    {
        await _dbSet.AddAsync(hall);
    }

    public void Remove(HallEntity hall)
    {
        _dbSet.Remove(hall);
    }

    // The same interval overlap principle as in Specification.OverlapsBooking (b.From < to && b.To > from),
// but written manually as a correlated subquery (h => !Any(...)) rather than through the Specification itself,
// because here the filter is applied to each hall h in a set of halls, not to a single booking.
// A hall is considered available if there are NO bookings for it that overlap the requested [from, to) interval.
    public async Task<List<HallEntity>> FindAvailableHallsAsync(
        DateTime from,
        DateTime to,
        int persons)
    {
        return await _dbSet
            .Where(h => h.Persons >= persons)
            .Where(h => !_context.Set<HallBookingEntity>()
                .Any(b =>
                    b.HallId == h.Id &&
                    b.From < to &&
                    b.To > from))
            .ToListAsync();
    }
}