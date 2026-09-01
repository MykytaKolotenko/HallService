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

    // The booking selection period is half-open: [from, to) — a booking is included in the report
// by its START date (b.From), not by the record creation date. Grouping is done by b.From.Date
// (without time), so all bookings from the same calendar day are collapsed into one report row.
// AsNoTracking() — this report is read-only, so EF does not need to track entity changes in the context.
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

    // 1) Get bookings for the period -> 2) expand each booking into its services (SelectMany over Favors,
//    where PriceAtBooking is the price frozen at booking time, see FavorMapper.ToEntity) ->
// 3) join with the Favors catalog only to get the current service name (Name may change after booking;
//    unlike price, the name is not "frozen") ->
// 4) group by (FavorId, Name) and calculate usage count and total revenue ->
// 5) sort by revenue descending and take the top N (limit).
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