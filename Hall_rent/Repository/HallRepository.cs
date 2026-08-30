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

    public async Task AddAsync(HallEntity hall)
    {
        await _dbSet.AddAsync(hall);
    }

    public async Task<HallEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public void Remove(HallEntity hall)
    {
        _dbSet.Remove(hall);
    }

    public async Task<List<HallEntity>> FindAvailableHallsAsync(DateTime startAt, DateTime endAt, int persons)
    {
        return await _dbSet
            .Where(h => h.Persons >= persons)
            .Where(h => !_context.Set<HallBookingEntity>().Any(b =>
                b.HallId == h.Id &&
                b.StartAt < endAt &&
                b.EndAt > startAt))
            .ToListAsync();
    }

    public async Task<HallEntity?> GetByIdWithFavorsAsync(Guid id)
    {
        return await _dbSet
            .Include(h => h.Favors)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
