using Hall_rent.Row;

namespace Hall_rent.Repository.Interfaces;

public interface IAnalyticsRepository
{
    Task<List<HallRevenueRow>> GetRevenueByPeriodAsync(DateTime startAt, DateTime endAt);
    Task<List<FavorRevenueRow>> GetTopFavorsAsync(DateTime from, DateTime to, int limit);
}