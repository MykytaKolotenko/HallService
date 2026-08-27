using Hall_rent.Repository.Hall;

namespace Hall_rent.Repository;

public class HallUnitOfWork : IHallUnitOfWork
{
    private readonly AppDbContext _context;

    public HallUnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }
}
