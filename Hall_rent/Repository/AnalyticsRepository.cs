using Hall_rent.Context;
using Hall_rent.Repository.Interfaces;
using Hall_rent.Row;
using Microsoft.EntityFrameworkCore;

namespace Hall_rent.Repository;

public class AnalyticsRepository : IAnalyticsRepository
{
    private readonly AppDbContext _context;

    public AnalyticsRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<HallRevenueRow>> GetByPeriodAsync(DateTime from, DateTime to)
    {
        return await _context.Bookings
            .AsNoTracking()
            .Where(b => b.From >= from && b.From < to)
            .GroupBy(b => b.From.Date)
            .OrderBy(g => g.Key)
            .Select(g => new HallRevenueRow(
                g.Key,
                g.Sum(b => b.Price),
                g.Count()))
            .ToListAsync();
    }

    public async Task<List<FavorRevenueRow>> GetTopFavorsAsync(
        DateTime from,
        DateTime to,
        int limit)
    {
        var result = await _context.Bookings
            .AsNoTracking()
            .Where(b => b.From >= from && b.From < to)
            .SelectMany(b => b.Favors)
            .Join(
                _context.Favors.AsNoTracking(),
                bf => bf.FavorId,
                f => f.Id,
                (bf, f) => new
                {
                    bf.FavorId,
                    f.Name,
                    bf.PriceAtBooking
                })
            .GroupBy(x => new
            {
                x.FavorId,
                x.Name
            })
            .Select(g => new FavorRevenueRow
            {
                Id = g.Key.FavorId,
                Name = g.Key.Name,
                BookingsCount = g.Count(),
                Revenue = g.Sum(x => x.PriceAtBooking)
            })
            .OrderByDescending(x => x.Revenue)
            .Take(limit)
            .ToListAsync();

        return result;
    }
}