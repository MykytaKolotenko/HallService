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
            .Select(g => new HallRevenueRow(g.Key, g.Sum(b => b.Price), g.Count()))
            .OrderBy(r => r.Day)
            .ToListAsync();
    }

    public async Task<List<FavorRevenueRow>> GetTopFavorsAsync(DateTime from, DateTime to, int limit)
    {
        var favorCounts = _context.Bookings
            .AsNoTracking()
            .Where(b => b.From >= from && b.From < to)
            .SelectMany(b => b.Favors, (booking, favorId) => favorId)
            .GroupBy(favorId => favorId)
            .Select(g => new FavorCount(g.Key.FavorId, g.Count()));

        return await JoinWithFavorDetails(favorCounts)
            .OrderByDescending(r => r.BookingsCount)
            .Take(limit)
            .ToListAsync();
    }

    private IQueryable<FavorRevenueRow> JoinWithFavorDetails(IQueryable<FavorCount> favorCounts)
    {
        return favorCounts.Join(
            _context.Favors,
            fc => fc.FavorId,
            favor => favor.Id,
            (fc, favor) => new FavorRevenueRow(favor.Id, fc.TimesBooked, favor.Name, fc.TimesBooked * favor.Price));
    }

    private record FavorCount(Guid FavorId, int TimesBooked);
}
