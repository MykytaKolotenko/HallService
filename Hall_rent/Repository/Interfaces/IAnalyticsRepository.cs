using Hall_rent.Row;

namespace Hall_rent.Repository.Interfaces;

public interface IAnalyticsRepository
{
    Task<List<HallRevenueRow>> GetByPeriodAsync(DateTime startAt, DateTime endAt);
    Task<List<FavorRevenueRow>> GetTopFavorsAsync(DateTime from, DateTime to, int limit);
}