using Hall_rent.Context;
using Hall_rent.Entity;
using Hall_rent.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class FavorRepository(AppDbContext context) : IFavorRepository
{
    private readonly DbSet<FavorEntity> _dbSet = context.Set<FavorEntity>();

    public async Task AddAsync(FavorEntity favor)
    {
        await _dbSet.AddAsync(favor);
    }

    public async Task<FavorEntity?> GetByIdAsync(Guid id)
    {
        return await _dbSet.Where(x => x.Id == id).FirstOrDefaultAsync();
    }

    public async Task<List<FavorEntity>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<List<FavorEntity>> GetByIdsAsync(List<Guid> ids)
    {
        return await _dbSet.Where(x => ids.Contains(x.Id)).ToListAsync();
    }

    public void Remove(FavorEntity favor)
    {
        _dbSet.Remove(favor);
    }
}